using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.Utilities;
using Yume.Data;
using Object = UnityEngine.Object;

namespace Yume.Battle {
  public enum CardHeapType {
    /// <summary>
    /// 抽牌堆
    /// </summary>
    DRAW,
    /// <summary>
    /// 手牌堆
    /// </summary>
    HAND,
    /// <summary>
    /// 弃牌堆
    /// </summary>
    DISCARD,
    /// <summary>
    /// 耗牌堆
    /// </summary>
    CONSUME,
  }
  
  public class Character : Unit {
    private List<Card> m_tempCardList = new List<Card>();
    protected Dictionary<CardHeapType, List<Card>> cardHeapDict = new Dictionary<CardHeapType, List<Card>>(); //牌堆(抽牌、弃牌、耗牌、手牌)

    public Character(UnitData character, CardData[] cardDatas) : base(character) {
      foreach (CardHeapType cardHeapType in Enum.GetValues(typeof(CardHeapType))) {
        cardHeapDict.Add(cardHeapType, new List<Card>());
      }
      
      foreach (var cardData in cardDatas) {
        Action<Card> onUseCard = card => {
          if (!card.active || !BattleManager.isRunning) {
            return;
          }
          
          EnqueueCard((card, BattleManager.GetNearestEnemy(card.gameObject.transform.position)));
          card.active = false;
        };
        cardHeapDict[CardHeapType.DRAW].Add(CardFactory.CreateCard(this, cardData, onUseCard));
      }
    }

    public override IEnumerator OnTick() {
      // 抽牌逻辑层
      int deltaCardCount = Constant.HAND_CARD_THRESHOLD - cardHeapDict[CardHeapType.HAND].Count;
      if (deltaCardCount > 0) {
        if (deltaCardCount > cardHeapDict[CardHeapType.DRAW].Count && cardHeapDict[CardHeapType.DISCARD].Count > 0) {
          // 弃牌堆的牌重新进入抽牌堆
          cardHeapDict[CardHeapType.DRAW].AddRange(cardHeapDict[CardHeapType.DISCARD]);
          cardHeapDict[CardHeapType.DISCARD].Clear();
        }
        
        Card[] randomCards = MathUtil.FisherYatesShuffle(cardHeapDict[CardHeapType.DRAW].ToArray());
        foreach (Card card in randomCards) {
          // 记录抽取的卡牌
          m_tempCardList.Add(card);
          cardHeapDict[CardHeapType.HAND].Add(card);
          cardHeapDict[CardHeapType.DRAW].Remove(card);
          if (cardHeapDict[CardHeapType.HAND].Count >= Constant.HAND_CARD_THRESHOLD) {
            break;
          }
        }
      }
      // 抽牌表现层
      yield return CardFactory.DrawCard(m_tempCardList.ToArray());
      m_tempCardList.Clear();
      // 激活手牌堆的牌
      cardHeapDict[CardHeapType.HAND].ForEach(card => card.active = true);
      
      // 出牌中
      while (true) {
        if (BattleManager.battleState != BattleState.RUNNING) {
          yield break;
        }
        if (playCardQueue.Count > 0) {
          var cardTuple = playCardQueue.Dequeue();
          if (cardTuple == default) {
            break;
          } 
          cardTuple.card.Cast(this, cardTuple.target);
          // 把用过的牌弃掉
          yield return CardFactory.DiscardCard(cardTuple.card);
        }
        yield return null;
      }
      
      // 弃牌逻辑层
      // 记录弃置的卡牌
      m_tempCardList.AddRange(cardHeapDict[CardHeapType.HAND]);
      cardHeapDict[CardHeapType.DISCARD].AddRange(m_tempCardList);
      cardHeapDict[CardHeapType.HAND].Clear();
      // 弃牌表现层
      yield return CardFactory.DiscardCard(m_tempCardList.ToArray());
      m_tempCardList.Clear();
    }
    
    public override void OnDie() {
      BattleManager.battleState = BattleState.LOSE;
    }

    public override void CleanUp() {
      cardHeapDict.Values.ForEach(
        cards =>
          cards.ForEach(card => Object.Destroy(card.gameObject))
      );
      Object.Destroy(gameObject);
    }
  }
}