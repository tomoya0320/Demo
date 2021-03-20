using System;
using System.Collections;
using System.Collections.Generic;
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
    private List<Card> m_tempCards = new List<Card>();
    protected override Transform folder => UnitFactory.charFolder;
    private List<IEnumerator> m_enumerators = new List<IEnumerator>(); // 其他主线程地方触发的协程
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
      cardHeapDict[CardHeapType.HAND].Remove(card);
      card.cardHeapType = CardHeapType.PLAYING;
      // 加入出牌队列
      EnqueueCard(card);

      return true;
    }

    public override IEnumerator OnTick() {
      // 抽牌逻辑层
      if (cardHeapDict[CardHeapType.HAND].Count < Constant.MAX_HAND_CARD_COUNT) {
        if (Constant.DRAW_CARD_COUNT > cardHeapDict[CardHeapType.DRAW].Count && cardHeapDict[CardHeapType.DISCARD].Count > 0) {
          // 弃牌堆的牌重新进入抽牌堆
          cardHeapDict[CardHeapType.DISCARD].ForEach(
            card => {
              card.gameObject.SetParent(CardFactory.GetFolder(CardHeapType.DRAW));
              card.rectTransform.anchoredPosition = Vector2.zero;
              card.cardHeapType = CardHeapType.DRAW;
            }
          );
          cardHeapDict[CardHeapType.DRAW].AddRange(cardHeapDict[CardHeapType.DISCARD]);
          cardHeapDict[CardHeapType.DISCARD].Clear();
        }

        Card[] randomCards = MathUtil.FisherYatesShuffle(cardHeapDict[CardHeapType.DRAW].ToArray());
        foreach (Card card in randomCards) {
          // 记录抽取的卡牌
          m_tempCards.Add(card);
          card.cardHeapType = CardHeapType.HAND;
          cardHeapDict[CardHeapType.HAND].Add(card);
          cardHeapDict[CardHeapType.DRAW].Remove(card);
          if (m_tempCards.Count >= Constant.DRAW_CARD_COUNT || cardHeapDict[CardHeapType.HAND].Count >= Constant.MAX_HAND_CARD_COUNT) {
            break;
          }
        }
      }
      // 抽牌表现层
      yield return _AddCards(m_tempCards.Count, cardHeapDict[CardHeapType.HAND].ToArray());
      m_tempCards.Clear();

      // 出牌中
      while (true) {
        if (playCardQueue.Count > 0) {
          var playCard = playCardQueue.Dequeue();
          if (playCard == null) {
            break;
          }
          // 出牌
          yield return playCard.Cast();
          // 执行主线程其他地方触发的协程
          foreach (var enumerator in m_enumerators) {
            yield return enumerator;
          }
          // 把用过的牌弃掉
          if (playCard.cardPhase.consume) {
            playCard.cardHeapType = CardHeapType.CONSUME;
            cardHeapDict[CardHeapType.CONSUME].Add(playCard);
          } else {
            playCard.cardHeapType = CardHeapType.DISCARD;
            cardHeapDict[CardHeapType.DISCARD].Add(playCard);
          }
          yield return _RemoveCards(playCard);
          // 重新排列剩下没有被拖拽的卡牌
          _ResortHandCards();
        }
        yield return null;
      }

      // 记录弃置的卡牌
      // 将不保留的手牌全部弃置
      m_tempCards.AddRange(cardHeapDict[CardHeapType.HAND].FindAll(card => !card.cardPhase.retain));
      m_tempCards.ForEach(card => {
          card.cardHeapType = CardHeapType.DISCARD;
          cardHeapDict[CardHeapType.HAND].Remove(card);
        }
      );
      // 放入弃牌堆
      cardHeapDict[CardHeapType.DISCARD].AddRange(m_tempCards);
      // 弃牌表现层
      yield return _RemoveCards(m_tempCards.ToArray());
      m_tempCards.Clear();
      // 重新排列剩下的手牌堆
      _ResortHandCards();
    }

    private IEnumerator _AddCards(int drawCardCount, params Card[] cards) {
      int drawCardStartIndex = cards.Length - drawCardCount;
      for (int i = 0; i < cards.Length; i++) {
        Vector2 dest = new Vector2((i - 0.5f * cards.Length + 0.5f) * cards[i].rect.width, 0);
        cards[i].battleCard.SetTargetAnchorPos(dest);
        if (i >= drawCardStartIndex) {
          cards[i].battleCard.stateMachine.SwitchState((int)BattleCardState.ADDING);
        }
        yield return new WaitForSeconds(0.1f);
      }
      // 避免迭代器失效所以传入的参数得是数组
      foreach (var card in cards) {
        yield return new WaitWhile(() => card.battleCard.GetCurrentState() == BattleCardState.ADDING);
      }
    }

    private IEnumerator _RemoveCards(params Card[] cards) {
      for (int i = cards.Length - 1; i >= 0; i--) {
        cards[i].battleCard.SetTargetAnchorPos(Vector2.zero);
        cards[i].battleCard.stateMachine.SwitchState((int)BattleCardState.REMOVING);
        yield return new WaitForSeconds(0.1f);
      }
      // 避免迭代器失效所以传入的参数得是数组
      foreach (var card in cards) {
        yield return new WaitWhile(() => card.battleCard.GetCurrentState() == BattleCardState.REMOVING);
      }
    }

    private void _ResortHandCards() {
      for (int i = 0; i < cardHeapDict[CardHeapType.HAND].Count; i++) {
        float width = cardHeapDict[CardHeapType.HAND][i].rect.width;
        Vector2 dest = new Vector2((i - 0.5f * cardHeapDict[CardHeapType.HAND].Count + 0.5f) * width, 0);
        cardHeapDict[CardHeapType.HAND][i].battleCard.SetTargetAnchorPos(dest);
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
      if (selector.to == CardHeapType.HAND) {
        int deltaCount = Constant.MAX_HAND_CARD_COUNT - cardHeapDict[CardHeapType.HAND].Count;
        if (cards.Count > deltaCount) {
          cards = cards.GetRange(0, deltaCount);
        }
      }

      cards.ForEach(
        card => {
          card.cardHeapType = selector.to;
          cardHeapDict[selector.from].Remove(card);
        }
      );
      cardHeapDict[selector.to].AddRange(cards);

      if (selector.to == CardHeapType.HAND) {
        m_enumerators.Add(_AddCards(cards.Count, cardHeapDict[CardHeapType.HAND].ToArray()));
      } else if (selector.from == CardHeapType.HAND) {
        m_enumerators.Add(_RemoveCards(cards.ToArray()));
      } else {
        cards.ForEach(
          card => {
            card.gameObject.SetParent(CardFactory.GetFolder(selector.to));
            card.rectTransform.anchoredPosition = Vector2.zero;
          }
        );
      }
    }

    public List<Card> GetCard(CardHeapType cardHeapType) {
      return cardHeapDict[cardHeapType];
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