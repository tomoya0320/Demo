using System.Collections.Generic;
using Yume.Data;

namespace Yume.Battle {
  public class Enemy : Unit {
    public Enemy(UnitData enemy, List<CardData> cardList) : base(enemy, cardList) {

    }
  }
}