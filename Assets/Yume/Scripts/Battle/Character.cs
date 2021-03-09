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
    private UICharCardManager m_uiCharCardManager = new UICharCardManager();
    protected override Transform folder => UnitFactory.charFolder;
    protected Dictionary<CardHeapType, List<Card>> cardHeapDict = new Dictionary<CardHeapType, List<Card>>(); //牌堆(抽牌、弃牌、耗牌、手牌)

    public Character(UnitData character, CardData[] cardDatas) : base(character) {
      foreach (CardHeapType cardHeapType in Enum.GetValues(typeof(CardHeapType))) {
        cardHeapDict.Add(cardHeapType, new List<Card>());
      }
      foreach (var cardData in cardDatas) {
        cardHeapDict[CardHeapType.DRAW].Add(CardFactory.CreateCard(this, cardData, _OnUseCard));
      }
    }

    private bool _OnUseCard(Card card) {
      if (card.candidate == null) {
        return false;
      }
      if (card.cardPhase.energyConsumption > GetAttrib(AttribType.ENERGY)) {
        if (card.candidate != null) {
          card.candidate.selected.SetActiveEx(false);
          card.candidate = null;
        }
        MessageManager.AddMessage(new Message(Message.Style.CENTER, "能量不足！", false, null, 0.5f));
        return false;
      }
      // 及时消耗能量
      if (card.cardPhase.energyConsumption > 0) {
        AddAttrib(AttribType.ENERGY, -card.cardPhase.energyConsumption);
      } else if (card.cardPhase.energyConsumption < 0) {
        float energy = GetAttrib(AttribType.ENERGY);
        AddAttrib(AttribType.ENERGY, -energy);
        blackboard[$"{card.cardTemplate.id}:eventCount"] = Mathf.RoundToInt(energy);
      }
      // 先关闭卡牌拖拽组件避免反复触发
      card.cardDrag.SetEnable(false);
      // 取消目标选中的UI
      card.candidate.selected.SetActiveEx(false);
      // 从手牌堆中移除
      cardHeapDict[CardHeapType.HAND].Remove(card);
      // 加入出牌队列
      EnqueueCard(card);
      return true;
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
      yield return m_uiCharCardManager.DrawCard(cardHeapDict[CardHeapType.HAND], m_tempCardList.Count);
      m_tempCardList.Clear();
      // 启用卡牌拖拽组件
      cardHeapDict[CardHeapType.HAND].ForEach(card => card.cardDrag?.SetEnable(true));

      // 出牌中
      while (true) {
        if (playCardQueue.Count > 0) {
          var playCard = playCardQueue.Dequeue();
          if (playCard == null) {
            break;
          }
          // 出牌
          yield return playCard.Cast();
          // 把用过的牌弃掉
          if (playCard.cardPhase.consume) {
            cardHeapDict[CardHeapType.CONSUME].Add(playCard); 
          } else {
            cardHeapDict[CardHeapType.DISCARD].Add(playCard); 
          }
          // 弃牌表现层
          yield return m_uiCharCardManager.DiscardCard(playCard.cardPhase.consume, playCard);
          // 重新排列剩下的手牌堆(动画过程中暂时关闭拖拽组件以防止被点击造成动画位置不正确)
          cardHeapDict[CardHeapType.HAND].ForEach(card => card.cardDrag?.SetEnable(false));
          yield return m_uiCharCardManager.ResortHandCard(cardHeapDict[CardHeapType.HAND]);
          cardHeapDict[CardHeapType.HAND].ForEach(card => card.cardDrag?.SetEnable(true));
        }
        yield return null;
      }
      
      // 记录弃置的卡牌
      // 默认是将手牌全部弃置
      m_tempCardList.AddRange(cardHeapDict[CardHeapType.HAND]);
      cardHeapDict[CardHeapType.HAND].Clear();
      // 放入弃牌堆
      cardHeapDict[CardHeapType.DISCARD].AddRange(m_tempCardList);
      // 弃牌表现层
      yield return m_uiCharCardManager.DiscardCard(false, m_tempCardList.ToArray());
      m_tempCardList.Clear();
      // 重新排列剩下的手牌堆
      yield return m_uiCharCardManager.ResortHandCard(cardHeapDict[CardHeapType.HAND]);
    }

    public void SetCardDragEnable(bool enable) {
      cardHeapDict[CardHeapType.HAND].ForEach(card => card.cardDrag.SetEnable(enable));
    }

    public List<CardData> GetCardDatas(CardHeapType type) {
      List<CardData> cardDatas = new List<CardData>();
      if (cardHeapDict.ContainsKey(type)) {
        foreach (var card in cardHeapDict[type]) {
          cardDatas.Add(card.data);
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