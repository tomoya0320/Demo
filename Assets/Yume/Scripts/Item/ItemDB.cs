using System;

namespace Yume.Battle {
  [Serializable]
  public class ItemDB {
    public int num;
    public string id;
    /// <summary>
    /// RedPoint用
    /// </summary>
    public bool isViewed;

    public ItemDB(string id, int num) {
      this.id = id;
      this.num = num;
    }
  }

  public struct ItemInfo {
    public string id;
    public string name;
    public string iconFileName;
    public string description;
  }
}