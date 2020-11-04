using System;

namespace Yume.Battle {
  [Serializable]
  public class CardDB {
    public string id;
  }

  public struct CardInfo {
    public string id;
    public string name;
    public string iconFileName;
    public string description;
  }
}