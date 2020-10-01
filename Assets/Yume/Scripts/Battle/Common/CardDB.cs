using System;

namespace Yume.Battle {
  [Serializable]
  public class CardDB {
    public string id;
  }

  public struct CardInfo {
    public enum Type {
      PHYSICAL_ATK, MAGIC_ATK, BUFF
    }

    public string id;
    public Type type;
    public string name;
    public string iconFileName;
    public string description;
  }
}