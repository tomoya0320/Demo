using System;
using System.Collections.Generic;
using Yume.Prototype;

namespace Yume.Data {
  [Serializable]
  public class ItemData {
    public string id;
    public int count;
    /// <summary>
    /// RedPoint用
    /// </summary>
    public bool isViewed;

    public ItemData(string id, int count) {
      this.id = id;
      this.count = count;
    }
  }

  [Serializable]
  public class MailData {
    public string id;
    public string title;
    public int timeStamp;
    public string content;
    public bool isReceived;
    public string signature;
    public List<ItemData> attachment = new List<ItemData>();

    public MailData(string id, string title, string content, string signature, params ItemData[] attachment) {
      this.id = id;
      this.title = title;
      this.content = content;
      this.signature = signature;
      if (attachment != null) {
        this.attachment.AddRange(attachment);
      }
      isReceived = this.attachment.Count == 0;
      timeStamp = MathUtil.GetTimeStamp(DateTime.Now);
    }
  }

  [Serializable]
  public class CardData {
    public string id;
    public int phase;

    public CardData(string id, int phase) {
      this.id = id;
      this.phase = phase;
    }
  }

  [Serializable]
  public class UnitData {
    public string id;
    public int level;
    public int passiveSkillLevels;

    public UnitData(string id, int level, int passiveSkillLevels) {
      this.id = id;
      this.level = level;
      this.passiveSkillLevels = passiveSkillLevels;
    }
  }

  [Serializable]
  public class TaskRequestData {
    public Method method;
    /// <summary>
    /// 完成任务后是否消耗所需物品
    /// </summary>
    public bool consumable;
    /// <summary>
    /// 是否忽略需求的ID
    /// </summary>
    public bool ignoreId;
    public RequestType type;
    public int currentCount;
    public int requiredCount;
    public string description;
    public string requirementId;
  }

  [Serializable]
  public class TaskData {
    public string id;
    public TaskType type;
    public TaskState state;
    public List<TaskRequestData> taskRequests;
  }

  [Serializable]
  public class TeamData {
    public string charId;
    public bool isSelected;
    public string name = "默认队伍名";
    public List<string> cardIdList = new List<string>();
  }

  [Serializable]
  public class UserData {
    public int lv;
    public int ap;
    public int exp;
    public int maxAp;
    public string name;
    public string account;
    public string password;
    public List<MailData> mails = new List<MailData>();
    public List<ItemData> items = new List<ItemData>();
    public List<CardData> cards = new List<CardData>();
    public List<UnitData> chars = new List<UnitData>();
    public List<TeamData> teams = new List<TeamData>();
    public Dictionary<TaskType, List<TaskData>> tasks;

    public UserData(string name = "", string account = "", string password = "") {
      this.name = name;
      this.account = account;
      this.password = password;
      if(DataManager.instance.TryGetMaxAp(lv, out int ret)) {
        ap = maxAp = ret;
      } else {
        ap = maxAp = 30;
      }

      tasks = DataManager.instance.GetDefaultTasks();

      for (int i = 0; i < Constant.MAX_TEAN_COUNT; i++) {
        teams.Add(new TeamData());
      }
      teams[0].isSelected = true;

#if TEST
      if (string.IsNullOrEmpty(name)) {
        this.name = "游客";
      }

      chars.Add(new UnitData("char_001_brave", 0, 0));
      chars.Add(new UnitData("char_002_wzb", 0, 0));

      cards.Add(new CardData("card_001_atk", 0));
      cards.Add(new CardData("card_001_atk", 0));
      cards.Add(new CardData("card_001_atk", 0));
      cards.Add(new CardData("card_001_atk", 0));
      cards.Add(new CardData("card_001_atk", 0));
      cards.Add(new CardData("card_001_atk", 1));
      cards.Add(new CardData("card_001_atk", 1));
      cards.Add(new CardData("card_001_atk", 1));
      cards.Add(new CardData("card_001_atk", 0));
      cards.Add(new CardData("card_001_atk", 1));
      cards.Add(new CardData("card_001_atk", 1));
      cards.Add(new CardData("card_001_atk", 0));
      cards.Add(new CardData("card_001_atk", 1));
      cards.Add(new CardData("card_001_atk", 1));
      cards.Add(new CardData("card_001_atk", 0));
      cards.Add(new CardData("card_001_atk", 1));
#endif
    }
  }
}