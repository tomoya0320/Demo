using System;
using System.Collections.Generic;
using UnityEngine;

namespace Yume.DB {
  [Serializable]
  public class UserDB {
    public string account;
    public List<ItemDB> items;

    public UserDB() {
      account = SystemInfo.deviceUniqueIdentifier;
      items = new List<ItemDB>();
    }
  }
}