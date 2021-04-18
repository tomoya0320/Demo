using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.Utilities;
using UnityEngine;
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
    /// <summary>
    /// 出牌堆
    /// </summary>
    PLAYING,
  }

  public class Character : Unit {
    public List<Card> retainCards = new List<Card>();
    private List<Card> m_tempCards = new List<Card>();
    protected override Transform folder => UnitFactory.charFolder;
    protected Dictionary<CardHeapType, List<Card>> cardHeapDict = new Dictionary<CardHeapType, List<Card>>(); //牌堆(抽牌、弃牌、耗牌、手牌)

    public Character(UnitArgs charArgs, CardArgs[] cardArgsArray) : base(charArgs) {
      foreach (CardHeapType cardHeapType in Enum.GetValues(typeof(CardHeapType))) {
        cardHeapDict.Add(cardHeapType, new List<Card>());
      }
      foreach (var cardArgs in cardArgsArray) {
        cardHeapDict[CardHeapType.DRAW].Add(CardFactory.CreateCard(this, cardArgs, _OnUseCard));
      }
    }

    private bool _OnUseCard(Card card) {
      // 点了回合结束但还有牌没出完的情况下不能再出牌了
      if (cardHeapDict[CardHeapType.PLAYING].Count > 0 &&
          cardHeapDict[CardHeapType.PLAYING].Last() == null) {
        
        return false;
      }
      // 回合外也不能出牌了
      if (BattleManager.currentUnit != this) {
        return false;
      }
      if (card.candidate == null) {
        return false;
      }
      if (!card.cardPhase.cardPlayer.TryPlay(card)) {
        return false;
      }

      // 从手牌堆中移除
      List<Card> handCards = cardHeapDict[CardHeapType.HAND];
      handCards.Remove(card);
      // 加入出牌队列
      EnqueueCard(card);
      card.cardHeapType = CardHeapType.PLAYING;
      // 调整SiblingIndex
      for (int i = 0; i < handCards.Count; i++) {
        handCards[i].battleCard.SetTargetSiblingIndex(i);
      }

      return true;
    }

    public override void OnUpdate() {
      base.OnUpdate();

      foreach (var cards in cardHeapDict.Values) {
        cards.ForEach(card => card?.OnUpdate());
      }
    }

    public override IEnumerator OnTick() {
      // 抽牌逻辑层
      if (Constant.DRAW_CARD_COUNT > cardHeapDict[CardHeapType.DRAW].Count && cardHeapDict[CardHeapType.DISCARD].Count > 0) {
        // 弃牌堆的牌重新进入抽牌堆
        cardHeapDict[CardHeapType.DISCARD].ForEach(
          card => {
            card.cardHeapType = CardHeapType.DRAW;
            card.battleCard.ResetTransform();
            card.battleCard.SwitchState(BattleCardState.OUT_HAND);
          }
        );
        cardHeapDict[CardHeapType.DRAW].AddRange(cardHeapDict[CardHeapType.DISCARD]);
        cardHeapDict[CardHeapType.DISCARD].Clear();
      }

      Card[] randomCards = MathUtil.FisherYatesShuffle(cardHeapDict[CardHeapType.DRAW]);
      foreach (Card card in randomCards) {
        // 记录抽取的卡牌
        m_tempCards.Add(card);
        card.cardHeapType = CardHeapType.HAND;
        cardHeapDict[CardHeapType.HAND].Add(card);
        cardHeapDict[CardHeapType.DRAW].Remove(card);
        if (m_tempCards.Count >= Constant.DRAW_CARD_COUNT) {
          break;
        }
      }
      // 抽牌表现层
      _AddCards(m_tempCards.Count, cardHeapDict[CardHeapType.HAND].ToArray());
      m_tempCards.Clear();

      // 出牌中
      while (true) {
        if (cardHeapDict[CardHeapType.PLAYING].Count > 0) {
          var playCard = cardHeapDict[CardHeapType.PLAYING].Dequeue();
          if (playCard == null) {
            break;
          }
          // 出牌
          yield return playCard.Cast();
          // 把用过的牌弃掉
          if (playCard.cardPhase.consume) {
            playCard.cardHeapType = CardHeapType.CONSUME;
            cardHeapDict[CardHeapType.CONSUME].Add(playCard);
          } else {
            playCard.cardHeapType = CardHeapType.DISCARD;
            cardHeapDict[CardHeapType.DISCARD].Add(playCard);
          }
          _RemoveCards(playCard);
          // 重新排列剩下没有被拖拽的卡牌
          _ResortHandCards();
        }
        yield return null;
      }
      
      // 结束回合前调用的行为树
      BattleManager.behaviorManagers[this].Run(BehaviorTime.ON_BEFORE_END_TURN);

      // 记录要保留的手牌
      retainCards.AddRange(cardHeapDict[CardHeapType.HAND].FindAll(card => card.cardPhase.retain));
      // 记录弃置的卡牌
      // 将不保留的手牌全部弃置
      m_tempCards.AddRange(cardHeapDict[CardHeapType.HAND].FindAll(card => !retainCards.Contains(card)));
      m_tempCards.ForEach(
        card => {
          card.cardHeapType = CardHeapType.DISCARD;
          cardHeapDict[CardHeapType.HAND].Remove(card);
        }
      );
      // 放入弃牌堆
      cardHeapDict[CardHeapType.DISCARD].AddRange(m_tempCards);
      // 弃牌表现层
      _RemoveCards(m_tempCards.ToArray());
      // 清空列表
      m_tempCards.Clear();
      retainCards.Clear();
      // 重新排列剩下的手牌堆
      _ResortHandCards();
    }

    private void _AddCards(int drawCardCount, params Card[] cards) {
      int drawCardStartIndex = cards.Length - drawCardCount;
      for (int i = 0; i < cards.Length; i++) {
        float scale = i - 0.5f * cards.Length + 0.5f;
        float angle = -scale * Constant.ANGLE_DELTA;
        float targetPosX = Constant.SPACING_SCALE * scale * CardFactory.cardWidth;
        float targetPosY = -Mathf.Abs(scale) * CardFactory.cardHeight * Constant.OFFSET_SCALE_Y;
        Vector2 dest = new Vector2(targetPosX, targetPosY);
        cards[i].battleCard.SetTarget(angle, i, dest);
        if (i >= drawCardStartIndex) {
          cards[i].battleCard.SwitchState(BattleCardState.ADDING);
        }
      }
    }

    private void _RemoveCards(params Card[] cards) {
      for (int i = 0; i < cards.Length; i++) {
        cards[i].battleCard.SetTarget(0, i, Vector2.zero);
        cards[i].battleCard.SwitchState(BattleCardState.REMOVING);
      }
    }

    private void _ResortHandCards() {
      List<Card> cards = cardHeapDict[CardHeapType.HAND];
      for (int i = 0; i < cards.Count; i++) {
        float scale = i - 0.5f * cards.Count + 0.5f;
        float angle = -scale * Constant.ANGLE_DELTA;
        float targetPosX = Constant.SPACING_SCALE * scale * CardFactory.cardWidth;
        float targetPosY = -Mathf.Abs(scale) * CardFactory.cardHeight * Constant.OFFSET_SCALE_Y;
        Vector2 dest = new Vector2(targetPosX, targetPosY);
        cards[i].battleCard.SetTarget(angle, i, dest);
      }
    }

    /// <summary>
    /// 卡牌效果
    /// 将某处的卡牌移到另一处
    /// </summary>
    /// <param name="selector"></param>
    public void TransferCards(CardSelector selector) {
      if (selector.from == selector.to) {
        Debug.LogWarning($"卡牌移动的起止位置相同！from:{selector.from.ToString()},to:{selector.to.ToString()}");
        return;
      }

      List<Card> cards = selector.Select(cardHeapDict[selector.from]);
      cards.ForEach(
        card => {
          card.cardHeapType = selector.to;
          cardHeapDict[selector.from].Remove(card);
        }
      );
      cardHeapDict[selector.to].AddRange(cards);

      if (selector.to == CardHeapType.HAND) {
        _AddCards(cards.Count, cardHeapDict[CardHeapType.HAND].ToArray());
      } else if (selector.from == CardHeapType.HAND) {
        _RemoveCards(cards.ToArray());
      } else {
        cards.ForEach(
          card => {
            card.battleCard.ResetTransform();
            card.battleCard.SwitchState(BattleCardState.OUT_HAND);
          }
        );
      }
    }

    public List<Card> GetCards(CardHeapType cardHeapType) {
      return cardHeapDict[cardHeapType];
    }

    public override void CleanUp() {
      cardHeapDict.Values.ForEach(cards => cards.ForEach(card => card.CleanUp()));
      Object.Destroy(gameObject);
    }

    public override void EnqueueCard(Card card) {
      cardHeapDict[CardHeapType.PLAYING].Add(card);
    }
  }
}