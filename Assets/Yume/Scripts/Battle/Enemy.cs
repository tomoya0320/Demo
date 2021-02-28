using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Yume.Data;
using Object = UnityEngine.Object;

namespace Yume.Battle {
  public class Enemy : Unit {
    private Tween m_cardTween;
    protected override Transform folder => UnitFactory.enemyFolder;
    protected List<Card> cards = new List<Card>(); // 敌人的卡牌都放一起

    public Enemy(UnitData enemy, CardData[] cardDatas) : base(enemy) {
      foreach (var cardData in cardDatas) {
        cards.Add(CardFactory.CreateCard(this, cardData, null, gameObject.transform));
      }
    }

    public override IEnumerator OnTick() {
      yield return new WaitForSeconds(1);
      // 筛选出符合出牌条件的牌
      List<Card> candidateCards = cards.FindAll(card => card.cardPhase.energyConsumption <= GetAttrib(AttribType.ENERGY));
      // 没有就直接结束回合
      if (candidateCards == null || candidateCards.Count == 0) {
        yield break;
      }
      // 随机选一张牌出
      int index = MathUtil.random.Next(0, candidateCards.Count);
      // 敌人默认的目标是玩家
      candidateCards[index].candidate = BattleManager.character;
      playCardQueue.Enqueue(candidateCards[index]);
      // 结束出牌的标记
      playCardQueue.Enqueue(null);
      // 出牌中
      while (true) {
        if (playCardQueue.Count > 0) {
          var card = playCardQueue.Dequeue();
          if (card == null) {
            break;
          }
          // TODO:测试用
          card.gameObject.SetActiveEx(true);
          card.gameObject.transform.localScale = 0.3f * Vector3.one;
          m_cardTween = card.gameObject.transform.DOScale(Vector3.one, 0.2f);
          yield return new WaitUntil(() => !m_cardTween.IsPlaying());
          yield return card.Cast();
          m_cardTween = card.gameObject.transform.DOScale(0.3f * Vector3.one, 0.2f);
          m_cardTween.OnComplete(() => card.gameObject.SetActiveEx(false));
          yield return new WaitUntil(() => !m_cardTween.IsPlaying());
        }
        yield return null;
      }
    }

    public override void OnDie() {
      // TODO:改进
      // 隐藏物体
      gameObject.SetActiveEx(false);
      // 移除这个单位施加的buff
      foreach (var buffManager in BattleManager.buffManagers.Values) {
        buffManager.OnCasterDie(this);
      }
      if (BattleManager.units.Count == 1) {
        BattleManager.Finish(BattleResult.WIN);
      }
    }

    public override void CleanUp() {
      cards.ForEach(card => Object.Destroy(card.gameObject));
      Object.Destroy(gameObject);
    }
  }
}