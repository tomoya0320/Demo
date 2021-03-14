using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Yume.Battle {
  public class Enemy : Unit {
    private CanvasGroup m_canvasGroup;
    protected override Transform folder => UnitFactory.enemyFolder;
    protected List<Card> cards = new List<Card>(); // 敌人的卡牌都放一起

    public Enemy(UnitArgs enemyArgs, CardArgs[] cardArgsArray) : base(enemyArgs) {
      m_canvasGroup = gameObject.GetComponentEx<CanvasGroup>();
      foreach (var cardArgs in cardArgsArray) {
        cards.Add(CardFactory.CreateCard(this, cardArgs, null, gameObject.transform));
      }
    }

    public override IEnumerator OnTick() {
      yield return new WaitForSeconds(1);
      // 筛选出符合出牌条件的牌
      List<Card> validCards = cards.FindAll(card => card.cardPhase.energyConsumption <= GetAttrib(AttribType.ENERGY));
      // 没有就直接结束回合
      if (validCards == null || validCards.Count == 0) {
        yield break;
      }
      // 随机选一张牌出
      int index = MathUtil.random.Next(0, validCards.Count);
      if (validCards[index].cardTemplate.onlySelf) {
        // candidate就是自己 
        validCards[index].candidate = this;
      } else {
        // 敌人默认的目标是玩家
        validCards[index].candidate = BattleManager.character;
      }
      // 消耗能量
      if (validCards[index].cardPhase.energyConsumption > 0) {
        AddAttrib(AttribType.ENERGY, -validCards[index].cardPhase.energyConsumption);
      }
      // 加入出牌队列
      playCardQueue.Enqueue(validCards[index]);
      // 结束出牌的标记
      playCardQueue.Enqueue(null);
      // 出牌中
      while (true) {
        if (playCardQueue.Count > 0) {
          var card = playCardQueue.Dequeue();
          if (card == null) {
            break;
          }
          
          card.gameObject.SetActiveEx(true);
          card.gameObject.transform.localScale = 0.3f * Vector3.one;
          var tween = card.gameObject.transform.DOScale(Vector3.one, 0.2f);
          yield return new WaitUntil(() => !tween.IsPlaying());
          yield return card.Cast();
          tween = card.gameObject.transform
                      .DOScale(0.3f * Vector3.one, 0.2f)
                      .OnComplete(() => card.gameObject.SetActiveEx(false));
          yield return new WaitUntil(() => !tween.IsPlaying());
        }
        yield return null;
      }
    }

    public override void OnDie() {
      // 隐藏
      m_canvasGroup.DOFade(0, 0.5f);
      // 结算判断
      if (BattleManager.units.Count == 1) {
        BattleManager.Finish(BattleResult.WIN);
      } else {
        // 移除这个单位施加的buff
        foreach (var buffManager in BattleManager.buffManagers.Values) {
          // 只针对还存活的单位
          if (buffManager.unit.isAlive) {
            buffManager.OnCasterDie(this);
          }
        }
      }
    }

    public override void CleanUp() {
      cards.ForEach(card => Object.Destroy(card.gameObject));
      Object.Destroy(gameObject);
    }
  }
}