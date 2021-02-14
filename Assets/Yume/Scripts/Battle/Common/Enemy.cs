using System.Collections;
using System.Collections.Generic;
using Yume.Data;

namespace Yume.Battle {
  public class Enemy : Unit {
    public Enemy(UnitData enemy, List<CardData> cardList) : base(enemy, BattleManager.enemyFolder) {
      foreach (var card in cardList) {
        cardHeapDict[CardHeapType.DRAW].Add(new Card(card));
      }
    }

    public override IEnumerator OnTrick() {
      yield return null;
    }
  }
}