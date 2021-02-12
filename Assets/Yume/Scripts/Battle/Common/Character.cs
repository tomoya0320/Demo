using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yume.Data;
using Yume.Resource;

namespace Yume.Battle {
  public class Character : Unit {
    public Character(UnitData character, List<CardData> cardList) : base(character, cardList, BattleManager.charFolder) {
      var prefab = ResourceManager.Load<GameObject>(StringMap.CARD_PREFAB);
      foreach (var card in cardList) {
        var go = Object.Instantiate(prefab, BattleManager.drawCardFolder);
        cardHeapDict[CardHeapType.DRAW].Add(new Card(card, go));
      }
    }

    public override IEnumerator OnTrick() {
      while (true) {
        if (cardQueue.Count > 0) {
          var card = cardQueue.Dequeue();
          if (card == null) {
            break;
          }

        }
        yield return null;
      }
    }
  }
}