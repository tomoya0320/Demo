using System;
using System.Collections.Generic;
using UnityEngine;
using Yume.MailSystem;
using Yume.String;
using Yume.Task;

namespace Yume.DB {
  [Serializable]
  public class UserDB {
    public string name;
    public string account;
    public string hashCode;
    public List<Mail> mails;
    public List<ItemDB> items;
    public Dictionary<string, List<TaskDB>> tasks;

    public UserDB() {
      name = StringMap.EMPTY;
      account = StringMap.EMPTY;
      hashCode = StringMap.EMPTY;
      mails = new List<Mail>();
      items = new List<ItemDB>();
      tasks = DBManager.GetDefaultTasks();
    }
  }
}