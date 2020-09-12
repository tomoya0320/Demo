using System;

namespace Yume.DB {
  [Serializable]
  public class ItemDB {
    public string id;
    public int num;

    public ItemDB(string id, int num) {
      this.id = id;
      this.num = num;
    }
  }
}