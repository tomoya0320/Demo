using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using UnityEngine;
using Yume.Task;

namespace Yume.DB {
  [Serializable]
  public class UserDB {
    public string account;
    public List<ItemDB> items;
    public Dictionary<string, List<TaskDB>> tasks;

    public UserDB() {
      account = SystemInfo.deviceUniqueIdentifier;
      items = new List<ItemDB>();
      tasks = DBManager.GetDefaultTasks();
    }
  }
}