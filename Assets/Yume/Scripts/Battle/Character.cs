using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.Utilities;
using UnityEngine;
using Yume.Data;
using Yume.UI;
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
    private UICardManager m_uiCardManager = new UICardManager();
    protected override Transform folder => UnitFactory.charFolder;
    protected Dictionary<CardHeapType, List<Card>> cardHeapDict = new Dictionary<CardHeapType, List<Card>>(); //牌堆(抽牌、弃牌、耗牌、手牌)

    public Character(UnitData character, CardData[] cardDatas) : base(character) {
      foreach (CardHeapType cardHeapType in Enum.GetValues(typeof(CardHeapType))) {
        cardHeapDict.Add(cardHeapType, new List<Card>());
      }

      foreach (var cardData in cardDatas) {
        Func<Card, bool> onUseCard = card => {
          if (card.candidate == null) {
            return false;
          }
          // 先关闭卡牌拖拽组件避免反复触发
          card.cardDrag?.SetEnable(false);
          card.candidate?.selected.SetActiveEx(false);
          // 从手牌堆移到弃牌堆
          if (cardHeapDict[CardHeapType.HAND].Remove(card)) {
            cardHeapDict[CardHeapType.DISCARD].Add(card);
          }
          m_uiCardManager.RemoveCards(card);
          EnqueueCard(card);
          return true;
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
      yield return m_uiCardManager.DrawCard(m_tempCardList.ToArray());
      // 启用卡牌拖拽组件
      cardHeapDict[CardHeapType.HAND].ForEach(card => card.cardDrag?.SetEnable(true));
      m_tempCardList.Clear();

      // 出牌中
      while (true) {
        if (playCardQueue.Count > 0) {
          var card = playCardQueue.Dequeue();
          if (card == null) {
            break;
          }
          // 出牌
          yield return card.Cast();
          // 把用过的牌弃掉
          yield return m_uiCardManager.DiscardCard(card);
        }
        yield return null;
      }
      
      // 记录弃置的卡牌
      m_tempCardList.AddRange(cardHeapDict[CardHeapType.HAND]);
      cardHeapDict[CardHeapType.DISCARD].AddRange(m_tempCardList);
      cardHeapDict[CardHeapType.HAND].Clear();
      // 弃牌表现层
      m_uiCardManager.RemoveCards(m_tempCardList.ToArray());
      yield return m_uiCardManager.DiscardCard(m_tempCardList.ToArray());
      m_tempCardList.Clear();
    }
    
    public void SetCardDragEnable(bool enable) {
      cardHeapDict[CardHeapType.HAND].ForEach(card => card.cardDrag?.SetEnable(enable));
    }

    public List<CardData> GetCardDatas(CardHeapType type) {
      List<CardData> cardDatas = new List<CardData>();
      if (cardHeapDict.ContainsKey(type)) {
        foreach (var card in cardHeapDict[type]) {
          cardDatas.Add(card.cardData);
        }
      }
      return cardDatas;
    }

    public override void OnDie() {
      BattleManager.Finish(BattleResult.LOSE);
    }

    public override void CleanUp() {
      cardHeapDict.Values.ForEach(cards => cards.ForEach(card => card.CleanUp()));
      Object.Destroy(gameObject);
    }
  }
}