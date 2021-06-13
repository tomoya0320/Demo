using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Yume.UI;

namespace Yume.Battle {
  public class Enemy : Unit {
    private Tween m_tween;
    private YWaitWhile m_waitCardAnim;
    protected override Transform folder => UnitFactory.enemyFolder;
    protected List<Card> cards = new List<Card>(); // 敌人的卡牌都放一起
    protected Queue<Card> playCardQueue = new Queue<Card>(); // 出牌队列

    public Enemy(UnitArgs enemyArgs, CardArgs[] cardArgsArray) : base(enemyArgs) {
      foreach (var cardArgs in cardArgsArray) {
        Card card = CardFactory.CreateCard(this, cardArgs, null, gameObject.transform);
        // 初始隐藏
        card.gameObject.SetActiveEx(false);
        cards.Add(card);
      }
      m_waitCardAnim = WaitUtil.GetWaitWhile(() => m_tween.IsPlaying());
      waitPlayCard = WaitUtil.GetWaitUntil(() => playCardQueue.Count > 0);
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
      if (validCards[index].cardPhase.onlySelf) {
        // candidate就是自己 
        validCards[index].candidate = this;
      } else {
        // 敌人默认的目标是玩家
        validCards[index].candidate = BattleManager.character;
      }
      // 加入出牌队列
      if (validCards[index].cardPhase.cardPlayer.TryPlay(validCards[index])) {
        playCardQueue.Enqueue(validCards[index]);
      }
      // 结束出牌的标记
      playCardQueue.Enqueue(null);
      // 出牌中
      while (true) {
        yield return waitPlayCard;
        var card = playCardQueue.Dequeue();
        if (card == null) {
          break;
        }
        card.gameObject.SetActiveEx(true);
        m_tween = card.gameObject.transform.DOScale(Vector3.one, 0.2f);
        yield return m_waitCardAnim;
        yield return card.Cast();
        m_tween = card.gameObject.transform
                      .DOScale(UIBattleCard.smallScale, 0.2f)
                      .OnComplete(() => card.gameObject.SetActiveEx(false));
        yield return m_waitCardAnim;
      }
    }

    public override void CleanUp() {
      base.CleanUp();
      
      WaitUtil.ReleaseWaitWhile(m_waitCardAnim);
      m_waitCardAnim = null;
      m_tween = null;
      cards.ForEach(card => card.CleanUp());
      cards.Clear();
    }

    public override void EnqueueCard(Card card) {
      base.EnqueueCard(card);
      
      playCardQueue.Enqueue(card);
    }
  }
}