using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yume.Data;
using Yume.Resource;
using DG.Tweening;
using Object = UnityEngine.Object;

namespace Yume.Battle {
  public static class CardFactory {
    private static Transform s_drawCardFolder;
    private static Transform s_handCardFolder;
    private static Transform s_discardCardFolder;
    private static Transform s_consumeCardFolder;
    private static List<Card> s_handCards = new List<Card>();
    
    public static void Init() {
      s_drawCardFolder = GameObject.Find("drawCardFolder").transform;
      s_handCardFolder = GameObject.Find("handCardFolder").transform;
      s_discardCardFolder = GameObject.Find("discardCardFolder").transform;
      s_consumeCardFolder = GameObject.Find("consumeCardFolder").transform;
    }

    public static Card CreateCard(Unit unit, CardData cardData, Action<Card> onUseCard = null, Transform parent = null) {
      var prefab = ResourceManager.Load<GameObject>("card_prefab");
      if (parent == null) {
        parent = s_drawCardFolder;
      }
      var go = Object.Instantiate(prefab, parent);
      return new Card(unit, cardData, go, onUseCard);
    }

    /// <summary>
    /// 抽牌动效=>从抽牌堆到手牌堆
    /// </summary>
    /// <returns></returns>
    public static IEnumerator DrawCard(params Card[] cards) {
      YTween tween = new YTween();
      s_handCards.AddRange(cards);
      for (int i = 0; i < s_handCards.Count; i++) {
        Card card = s_handCards[i];
        card.gameObject.SetActiveEx(true);
        card.gameObject.SetParent(s_handCardFolder);
        tween.count++;
        card.gameObject.transform.DOScale(Vector2.one, 0.5f).OnComplete(() => tween.count--);
        tween.count++;
        Vector2 dest = new Vector2((i - 0.5f * s_handCards.Count + 0.5f) * card.rect.width, 0);
        card.gameObject.GetRectTransform().DOAnchorPos(dest, 0.5f).OnComplete(() => tween.count--);
        if (i < s_handCards.Count - 1) {
          yield return new WaitForSeconds(0.1f);
        }
      }

      yield return new WaitUntil(() => tween.count == 0);
    }

    /// <summary>
    /// 弃牌动效=>从手牌堆到弃牌堆
    /// </summary>
    /// <returns></returns>
    public static IEnumerator DiscardCard(params Card[] cards) {
      YTween tween = new YTween();
      for (int i = cards.Length - 1; i >= 0; i--) {
        s_handCards.Remove(cards[i]);
        cards[i].gameObject.SetParent(s_discardCardFolder);
        Transform transform = cards[i].gameObject.transform;
        tween.count++;
        transform.DOScale(0.3f * Vector2.one, 0.5f).OnComplete(() => tween.count--);
        tween.count++;
        transform.DOLocalMove(Vector2.zero, 0.5f).OnComplete(
          () => {
            transform.gameObject.SetActiveEx(false);
            tween.count--;
          }
        );
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
    public static IEnumerator ConsumeCard(params Card[] cards) {
      yield return null;
    }

    public static void CleanUp() {
      s_drawCardFolder = null;
      s_handCardFolder = null;
      s_discardCardFolder = null;
      s_consumeCardFolder = null;
      s_handCards.Clear();
    }
  }
}