using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Yume.UI;

namespace Yume.Battle {
  public class Enemy : Unit {
    protected Tween tween;
    protected YWaitWhile waitCardAnim;
    protected Queue<Card> playCards = new Queue<Card>();
    protected override Transform folder => UnitFactory.enemyFolder;

    public Enemy(UnitArgs enemyArgs, CardArgs[] cardArgsArray) : base(enemyArgs) {
      // 初始化卡牌
      foreach (var cardArgs in cardArgsArray) {
        Card card = CardFactory.CreateCard(this, cardArgs, null, gameObject.transform.Find("cardFolder"));
        // 初始化时隐藏卡牌
        card.gameObject.SetActiveEx(false);
        // 默认都放手牌堆里
        cardHeapDict[CardHeapType.HAND].Add(card);
      }
      
      waitCardAnim = WaitUtil.GetWaitWhile(() => tween.IsPlaying());
    }

    public override IEnumerator OnTick() {
      yield return new WaitForSeconds(1);
      // 敌方出牌逻辑
      Card[] randomCards = MathUtil.FisherYatesShuffle(cardHeapDict[CardHeapType.HAND]);
      foreach (Card card in randomCards) {
        if (card.cardPhase.energyConsumption > GetAttrib(AttribType.ENERGY)) {
          continue;
        }
        
        if (card.cardPhase.onlySelf) {
          // candidate就是自己 
          card.candidate = this;
        } else {
          // 敌人默认的目标是玩家
          card.candidate = BattleManager.character;
        }
        if (card.cardPhase.cardPlayer.TryPlay(card)) {
          // 加入出牌队列
          playCards.Enqueue(card);
          break;
        }
      }

      // 出牌中
      while (playCards.Count > 0) {
        var playCard = playCards.Dequeue();
        // 出牌
        playCard.gameObject.SetActiveEx(true);
        tween = playCard.gameObject.transform.DOScale(Vector3.one, 0.2f);
        yield return waitCardAnim;
        yield return playCard.Cast();
        tween = playCard.gameObject.transform
                          .DOScale(UIBattleCard.smallScale, 0.2f)
                          .OnComplete(() => playCard.gameObject.SetActiveEx(false));
        yield return waitCardAnim;
      }

      // 结束回合前调用的行为树
      BattleManager.behaviorManagers[this].Run(BehaviorTime.ON_BEFORE_END_TURN);
    }

    public override void CleanUp() {
      base.CleanUp();
      
      WaitUtil.ReleaseWaitWhile(waitCardAnim);
      waitCardAnim = null;
      tween = null;
      playCards.Clear();
    }
  }
}