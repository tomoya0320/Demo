using System;
using UnityEngine;
using Yume.UI;
using Object = UnityEngine.Object;

namespace Yume.Battle {
  public static class CardFactory {
    private static GameObject s_cardPrefab;
    private static Transform s_drawCardFolder;
    private static Transform s_handCardFolder;
    private static Transform s_cardAnimFolder;
    private static Transform s_discardCardFolder;
    private static Transform s_consumeCardFolder;
    private static Transform s_handCardTopFolder;
    public static float cardWidth { private set; get; }
    public static float cardHeight { private set; get; }
    public static Transform cardAnimFolder => s_cardAnimFolder;
    public static Transform handCardTopFolder => s_handCardTopFolder;

    public static void Init(ObjectContainer objectContainer) {
      s_cardAnimFolder = objectContainer.GetTransform("cardAnimFolder");
      s_drawCardFolder = objectContainer.GetTransform("drawCardFolder");
      s_handCardFolder = objectContainer.GetTransform("handCardFolder");
      s_discardCardFolder = objectContainer.GetTransform("discardCardFolder");
      s_consumeCardFolder = objectContainer.GetTransform("consumeCardFolder");
      s_handCardTopFolder = objectContainer.GetTransform("handCardTopFolder");

      s_cardPrefab = ResourceManager.Load<GameObject>("card_prefab");
      cardWidth = s_cardPrefab.GetComponent<RectTransform>().rect.width;
      cardHeight = s_cardPrefab.GetComponent<RectTransform>().rect.height;
    }

    public static Transform GetFolder(CardHeapType cardHeapType) {
      switch (cardHeapType) {
        case CardHeapType.DRAW:
          return s_drawCardFolder;
        case CardHeapType.HAND:
          return s_handCardFolder;
        case CardHeapType.DISCARD:
          return s_discardCardFolder;
        case CardHeapType.CONSUME:
          return s_consumeCardFolder;
        default:
          return null;
      }
    }

    public static Card CreateCard(Unit unit, CardArgs cardArgs, Func<Card, bool> onUseCard = null, Transform parent = null) {
      if (parent == null) {
        parent = s_drawCardFolder;
      }
      GameObject go = Object.Instantiate(s_cardPrefab, parent);
      go.transform.localScale = UIBattleCard.smallScale;
      return new Card(unit, cardArgs, go, onUseCard);
    }

    public static void CleanUp() {
      for (int i = 0; i < s_cardAnimFolder.childCount; i++) {
        Object.Destroy(s_cardAnimFolder.GetChild(i).gameObject);
      }
      s_cardPrefab = null;
      s_cardAnimFolder = null;
      s_drawCardFolder = null;
      s_handCardFolder = null;
      s_discardCardFolder = null;
      s_consumeCardFolder = null;
      s_handCardTopFolder = null;
    }
  }
}