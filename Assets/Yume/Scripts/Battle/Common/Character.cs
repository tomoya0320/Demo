using System.Collections.Generic;
using Yume.Data;

namespace Yume.Battle {
  public class Character : Unit {
    public Character(UnitData character, List<CardData> cardList) : base(character, cardList) {

    }
  }
}