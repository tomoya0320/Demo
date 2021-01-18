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
    public string name;
    public string description;
    public string iconFileName;
    public int energyConsumption;
    public Blackboard blackboard;
  }

  [Serializable]
  public struct CardInfo {
    public string id;
    public CardType cardType;
    public CardPhase[] cardPhases;
  }
}