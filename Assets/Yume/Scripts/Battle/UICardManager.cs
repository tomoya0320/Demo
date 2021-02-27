using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yume.Battle;

namespace Yume.UI {
  public class UICardManager {
    private const float CARD_SCALE_DURATION = 0.5f;
    private const float CARD_MOVE_DURATION = 0.5f;
    private List<Card> m_cards = new List<Card>();
    
    /// <summary>
    /// 抽牌动效=>从抽牌堆到手牌堆
    /// 仅限玩家
    /// </summary>
    /// <returns></returns>
    public IEnumerator DrawCard(params Card[] cards) {
      YTween tween = new YTween();
      m_cards.AddRange(cards);
      for (int i = 0; i < m_cards.Count; i++) {
        Card card = m_cards[i];
        card.gameObject.SetActiveEx(true);
        card.gameObject.SetParent(CardFactory.handCardFolder);
        tween.DOScale(card.gameObject, Vector3.one, CARD_SCALE_DURATION);
        Vector2 dest = new Vector2((i - 0.5f * m_cards.Count + 0.5f) * card.rect.width, 0);
        tween.DOAnchorPos(card.gameObject, dest, CARD_MOVE_DURATION);
        if (i < m_cards.Count - 1) {
          yield return new WaitForSeconds(0.1f);
        }
      }

      yield return new WaitUntil(() => tween.count == 0);
    }

    /// <summary>
    /// 弃牌动效=>从手牌堆到弃牌堆
    /// 仅限玩家
    /// </summary>
    /// <returns></returns>
    public IEnumerator DiscardCard(params Card[] cards) {
      YTween tween = new YTween();
      for (int i = cards.Length - 1; i >= 0; i--) {
        m_cards.Remove(cards[i]);
        cards[i].gameObject.SetParent(CardFactory.discardCardFolder);
        Transform transform = cards[i].gameObject.transform;
        tween.DOScale(transform,0.3f * Vector3.one, CARD_SCALE_DURATION);
        tween.DOLocalMove(transform, Vector3.zero, CARD_MOVE_DURATION, () => transform.gameObject.SetActiveEx(false));
        if (i > 0) {
          yield return new WaitForSeconds(0.1f);
        }
      }

      yield return new WaitUntil(() => tween.count == 0);
    }

    /// <summary>
    /// 耗牌动效=>从手牌堆到耗牌堆
    /// </summary>
    /// <returns></returns>
    public IEnumerator ConsumeCard(params Card[] cards) {
      yield return null;
    }
  }
}