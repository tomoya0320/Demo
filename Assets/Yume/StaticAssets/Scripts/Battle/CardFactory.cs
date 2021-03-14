using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Yume.Battle {
  public static class CardFactory {
    public static Transform cardAnimFolder { private set; get; }
    public static Transform drawCardFolder { private set; get; }
    public static Transform handCardFolder { private set; get; }
    public static Transform discardCardFolder { private set; get; }
    public static Transform consumeCardFolder { private set; get; }

    public static void Init(ObjectContainer objectContainer) {
      cardAnimFolder = objectContainer.GetTransform("cardAnimFolder");
      drawCardFolder = objectContainer.GetTransform("drawCardFolder");
      handCardFolder = objectContainer.GetTransform("handCardFolder");
      discardCardFolder = objectContainer.GetTransform("discardCardFolder");
      consumeCardFolder = objectContainer.GetTransform("consumeCardFolder");
    }

    public static Card CreateCard(Unit unit, CardArgs cardArgs, Func<Card, bool> onUseCard = null, Transform parent = null) {
      var prefab = ResourceManager.Load<GameObject>("card_prefab");
      if (parent == null) {
        parent = drawCardFolder;
      }
      var go = Object.Instantiate(prefab, parent);
      return new Card(unit, cardArgs, go, onUseCard);
    }

    public static void CleanUp() {
      for (int i = 0; i < cardAnimFolder.childCount; i++) {
        Object.Destroy(cardAnimFolder.GetChild(i).gameObject);
      }
      cardAnimFolder = null;
      drawCardFolder = null;
      handCardFolder = null;
      discardCardFolder = null;
      consumeCardFolder = null;
    }
  }
}