using System;
using System.Collections.Generic;
using Yume.Battle;
using Yume.MailSystem;
using Yume.Task;
using Yume.UI;

namespace Yume.DB {
  [Serializable]
  public class UserDB {
    public int lv;
    public int ap;
    public int exp;
    public int maxAp;
    public string name;
    public string account;
    public string password;
    public List<MailDB> mails = new List<MailDB>();
    public List<ItemDB> items = new List<ItemDB>();
    public List<CardDB> cards = new List<CardDB>();
    public List<UnitDB> chars = new List<UnitDB>();
    public List<TeamDB> teams = new List<TeamDB>();
    public Dictionary<TaskType, List<TaskDB>> tasks;

    public UserDB(string name = "", string account = "",string password = "") {
      this.name = name;
      this.account = account;
      this.password = password;

      tasks = DBManager.instance.GetDefaultTasks();

      for (int i = 0; i < Constant.MAX_TEAN_COUNT; i++) {
        teams.Add(new TeamDB());
      }
      teams[0].isSelected = true;

      if (DBManager.instance.TryGetMaxApBy(lv, out int ret)) {
        ap = maxAp = ret;
      } else {
        ap = maxAp = 30;
      }

#if TEST
      if (string.IsNullOrEmpty(name)) {
        this.name = "游客";
      }

      chars.Add(new UnitDB("char_001_brave", 0, 0));
      chars.Add(new UnitDB("char_002_wzb", 0, 0));

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
      cards.Add(new CardDB("card_001_atk",1));
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