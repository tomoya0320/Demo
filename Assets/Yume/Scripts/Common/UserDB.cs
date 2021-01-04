using System;
using System.Collections.Generic;
using Yume.Battle;
using Yume.MailSystem;
using Yume.Task;

namespace Yume.DB {
  [Serializable]
  public class UserDB {
    public int exp;
    public int lv = 1;
    public int ap = 30;
    public int maxAp = 30;
    public string name;
    public string account;
    public string password;
    public List<MailDB> mails = new List<MailDB>();
    public List<ItemDB> items = new List<ItemDB>();
    public List<CardDB> cards = new List<CardDB>();
    public List<UnitDB> chars = new List<UnitDB>();
    public Dictionary<TaskType, List<TaskDB>> tasks;

    public UserDB(string name = "", string account = "",string password = "") {
      this.name = name;
      this.account = account;
      this.password = password;
      tasks = DBManager.instance.GetDefaultTasks();
      if (DBManager.instance.TryGetMaxApBy(lv, out int maxAp)) {
        ap = maxAp;
        this.maxAp = maxAp;
      }

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