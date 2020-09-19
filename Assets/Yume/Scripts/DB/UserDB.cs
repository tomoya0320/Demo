using System;
using System.Collections.Generic;
using UnityEngine;
using Yume.String;
using Yume.Task;

namespace Yume.DB {
  [Serializable]
  public class UserDB {
    public string account;
    public string hashCode;
    public List<ItemDB> items;
    public Dictionary<string, List<TaskDB>> tasks;

    public UserDB() {
      account = StringMap.EMPTY;
      hashCode = StringMap.EMPTY;
      items = new List<ItemDB>();
      tasks = DBManager.GetDefaultTasks();
    }
  }
}