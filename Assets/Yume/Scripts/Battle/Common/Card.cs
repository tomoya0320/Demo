using System;
using System.ComponentModel;

namespace Yume.Battle {
  [Serializable]
  public class CardDB {
    public string id;
    public int phase;

    public CardDB(string id, int phase) {
      this.id = id;
      this.phase = phase;
    }
  }

  [Serializable]
  public struct CardInfo {
    public enum CardType {
      [Description("攻击")]
      ATTACK,
      [Description("技能")]
      SKILL,
      [Description("诅咒")]
      CURSE
    }

    [Serializable]
    public struct CardPhase {
      public string description;
      public int energyConsumption;
      public BuffData[] buffData;
      public Blackboard blackboard;
    }

    public string id;
    public string name;
    public CardType cardType;
    public string iconFileName;
    public CardPhase[] cardPhases;
  }
}