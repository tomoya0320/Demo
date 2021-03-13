using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yume.Battle;

namespace Yume.UI {
  public class UICharCardManager {
    private const float CARD_MOVE_DURATION = 0.5f;
    private const float CARD_SCALE_DURATION = 0.5f;

    /// <summary>
    /// 从抽牌堆移动到手牌堆的动效
    /// 仅限玩家
    /// </summary>
    /// <returns></returns>
    public IEnumerator DrawCard(List<Card> handCards, int drawCardCount) {
      int originHandCardCount = handCards.Count - drawCardCount;
      YTween tween = new YTween();
      for (int i = 0; i < handCards.Count; i++) {
        Card card = handCards[i];
        if (i >= originHandCardCount) {
          card.gameObject.SetActiveEx(true);
          card.gameObject.SetParent(CardFactory.handCardFolder);
          card.gameObject.transform.localScale = 0.3f * Vector3.one;
          tween.DOScale(card.gameObject, Vector3.one, CARD_SCALE_DURATION);
        }
        Vector2 dest = new Vector2((i - 0.5f * handCards.Count + 0.5f) * card.rect.width, 0);
        card.cardDrag.SetResetAnchorPos(dest);
        tween.DOAnchorPos(card.gameObject, dest, CARD_MOVE_DURATION);
        yield return new WaitForSeconds(0.1f);
      }

      yield return new WaitUntil(() => tween.count == 0);
    }

    /// <summary>
    /// 从手牌堆移动到弃/耗牌堆的动效
    /// 仅限玩家
    /// </summary>
    /// <returns></returns>
    public IEnumerator DiscardCard(bool consume, params Card[] cards) {
      YTween tween = new YTween();
      for (int i = cards.Length - 1; i >= 0; i--) {
        GameObject gameObject = cards[i].gameObject;
        if (consume) {
          gameObject.SetParent(CardFactory.consumeCardFolder);
        } else {
          gameObject.SetParent(CardFactory.discardCardFolder);
        }
        tween.DOScale(gameObject.transform, 0.3f * Vector3.one, CARD_SCALE_DURATION);
        tween.DOLocalMove(gameObject.transform, Vector3.zero, CARD_MOVE_DURATION).onComplete += () => gameObject.SetActiveEx(false);
        yield return new WaitForSeconds(0.1f);
      }

      yield return new WaitUntil(() => tween.count == 0);
    }

    /// <summary>
    /// 重新排列手牌堆
    /// </summary>
    /// <param name="handCards"></param>
    /// <returns></returns>
    public IEnumerator ResortHandCard(List<Card> handCards) {
      YTween tween = new YTween();
      for (int i = 0; i < handCards.Count; i++) {
        Card card = handCards[i];
        Vector2 dest = new Vector2((i - 0.5f * handCards.Count + 0.5f) * card.rect.width, 0);
        card.cardDrag.SetResetAnchorPos(dest);
        // 如果该卡牌正在被拖拽中就不移动
        if (!card.cardDrag.dragging) {
          tween.DOAnchorPos(card.gameObject, dest, 0.3f * CARD_MOVE_DURATION);
        }
      }

      yield return new WaitUntil(() => tween.count == 0);
    }
  }
}