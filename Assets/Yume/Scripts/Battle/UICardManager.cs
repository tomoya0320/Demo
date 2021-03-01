using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yume.Battle;

namespace Yume.UI {
  public class UICardManager {
    private const float CARD_MOVE_DURATION = 0.5f;
    private const float CARD_SCALE_DURATION = 0.5f;
    private List<Card> m_handCards = new List<Card>();

    /// <summary>
    /// 从抽牌堆移动到手牌堆的动效
    /// 仅限玩家
    /// </summary>
    /// <returns></returns>
    public IEnumerator DrawCard(params Card[] cards) {
      int handCardCount = m_handCards.Count;
      YTween tween = new YTween();
      m_handCards.AddRange(cards);
      for (int i = 0; i < m_handCards.Count; i++) {
        Card card = m_handCards[i];
        if (i >= handCardCount) {
          card.gameObject.SetActiveEx(true);
          card.gameObject.SetParent(CardFactory.handCardFolder);
          card.gameObject.transform.localScale = 0.3f * Vector3.one;
          tween.DOScale(card.gameObject, Vector3.one, CARD_SCALE_DURATION);
        }
        Vector2 dest = new Vector2((i - 0.5f * m_handCards.Count + 0.5f) * card.rect.width, 0);
        card.cardDrag?.SetResetAnchorPos(dest);
        tween.DOAnchorPos(card.gameObject, dest, CARD_MOVE_DURATION);
        yield return new WaitForSeconds(0.1f);
      }

      yield return new WaitUntil(() => tween.count == 0);
    }

    /// <summary>
    /// 从手牌堆移动到弃牌堆的动效
    /// 仅限玩家
    /// </summary>
    /// <returns></returns>
    public IEnumerator DiscardCard(params Card[] cards) {
      YTween tween = new YTween();
      foreach (var card in cards) {
        card.gameObject.SetParent(CardFactory.discardCardFolder);
        GameObject gameObject = card.gameObject;
        tween.DOScale(gameObject.transform,0.3f * Vector3.one, CARD_SCALE_DURATION);
        tween.DOLocalMove(gameObject.transform, Vector3.zero, CARD_MOVE_DURATION, () => gameObject.SetActiveEx(false));
        yield return new WaitForSeconds(0.1f);
      }
      // 重新排列剩下的手牌
      for (int i = 0; i < m_handCards.Count; i++) {
        Card card = m_handCards[i];
        Vector2 dest = new Vector2((i - 0.5f * m_handCards.Count + 0.5f) * card.rect.width, 0);
        card.cardDrag?.SetResetAnchorPos(dest);
        tween.DOAnchorPos(card.gameObject, dest, 0.5f * CARD_MOVE_DURATION);
        yield return new WaitForSeconds(0.05f);
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

    /// <summary>
    /// 逻辑上从手牌堆移除
    /// 因为移除的表现和逻辑不一定是同步的
    /// 但添加的表现和逻辑是同步
    /// </summary>
    /// <param name="cards"></param>
    public void RemoveCards(params Card[] cards) {
      foreach (var card in cards) {
        m_handCards.Remove(card);
      }
    }
  }
}