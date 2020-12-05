using System;

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
    public enum Type {
      ATTACK, SKILL, CURSE
    }

    [Serializable]
    public struct Phase {
      public string description;
      public int energyConsumption;
      public BuffData[] buffData;
      public Blackboard blackboard;
    }

    public string id;
    public string name;
    public string iconFileName;
    public Type type;
    public Phase[] phases;
  }
}