using System;
using System.Collections.Generic;
using UnityEngine;
using Yume.String;
using Yume.Task;

namespace Yume.DB {
  [Serializable]
  public class UserDB {
    public string account;
    public string password;
    public List<ItemDB> items;
    public Dictionary<string, List<TaskDB>> tasks;

    public UserDB() {
      account = SystemInfo.deviceUniqueIdentifier;
      password = StringMap.EMPTY;
      items = new List<ItemDB>();
      tasks = DBManager.GetDefaultTasks();
    }
  }
}