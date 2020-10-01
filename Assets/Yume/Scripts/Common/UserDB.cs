using System;
using System.Collections.Generic;
using Yume.Battle;
using Yume.MailSystem;
using Yume.String;
using Yume.Task;

namespace Yume.DB {
  [Serializable]
  public class UserDB {
    public int ap;
    public int maxAp;
    public int lv;
    public int exp;
    public string name;
    public string account;
    public string password;
    public List<MailDB> mails;
    public List<ItemDB> items;
    public List<CardDB> cards;
    public Dictionary<string, List<TaskDB>> tasks;

    public UserDB(string name = "", string account = "",string password = "") {
      ap = 30;
      maxAp = 30;
      lv = 1;
      exp = 0;
      this.name = name;
      this.account = account;
      this.password = password;
      mails = new List<MailDB>();
      items = new List<ItemDB>();
      cards = new List<CardDB>();
      tasks = DBManager.GetDefaultTasks();
    }
  }
}