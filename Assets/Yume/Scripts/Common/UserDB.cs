using System;
using System.Collections.Generic;
using Yume.Battle;
using Yume.MailSystem;
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
    public Dictionary<TaskType, List<TaskDB>> tasks;

    public UserDB(string name = "", string account = "",string password = "") {
      lv = 1;
      exp = 0;
      ap = 30;
      maxAp = 30;
      this.name = name;
      this.account = account;
      this.password = password;
      mails = new List<MailDB>();
      items = new List<ItemDB>();
      cards = new List<CardDB>();
      tasks = DBManager.instance.GetDefaultTasks();

#if TEST
      this.name = "admin";
      cards.Add(new CardDB("card_001_atk",0));
      cards.Add(new CardDB("card_001_atk",0));
      cards.Add(new CardDB("card_001_atk",0));
      cards.Add(new CardDB("card_001_atk",0));
      cards.Add(new CardDB("card_001_atk",0));
      cards.Add(new CardDB("card_001_atk",1));
      cards.Add(new CardDB("card_001_atk",1));
      cards.Add(new CardDB("card_001_atk",1));
      cards.Add(new CardDB("card_001_atk",1));
      cards.Add(new CardDB("card_001_atk",1));
      cards.Add(new CardDB("card_001_atk",1));
      cards.Add(new CardDB("card_001_atk",1));
#endif
    }
  }
}