using System;

namespace Yume.Battle {
  [Serializable]
  public class CardDB {
    public string id;
    public int phase;
  }

  public struct CardInfo {
    public enum Type {
      ATTACK, SKILL, ABILITY, CURSE
    }

    public struct Phase {
      public float atk;
    }

    public string id;
    public Type type;
    public string name;
    public string iconFileName;
    public string description;

  }
}