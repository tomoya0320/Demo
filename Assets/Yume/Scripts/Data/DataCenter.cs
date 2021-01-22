using System;
using System.Collections.Generic;
using System.ComponentModel;
using Yume.Battle;
using Yume.Resource;

namespace Yume.Data {
  #region Template
  public struct ItemTemplate {
    public string id;
    public string name;
    public string iconFileName;
    public string description;
  }

  public enum CardType {
    [Description("攻击")]
    ATTACK,
    [Description("技能")]
    SKILL,
    [Description("诅咒")]
    CURSE
  }

  public struct CardPhase {
    public string name;
    public string description;
    public string iconFileName;
    public int energyConsumption;
    public Blackboard blackboard;
  }

  public struct CardTemplate {
    public string id;
    public CardType cardType;
    public CardPhase[] cardPhases;
  }

  public struct PassiveSkillPhase {
    public string name;
    public string description;
    public string iconFileName;
    public Blackboard blackboard;
  }

  public struct UnitTemplate {
    public string id;
    public string tag;
    public string name;
    public int maxLevel;
    public string prefabName;
    public string portraitFileName;
    public string illustrationFileName;
    public PassiveSkillPhase[] passiveSkillPhases;
    public Dictionary<AttribType, Attrib> minAttribDict;
    public Dictionary<AttribType, Attrib> maxAttribDict;

    public Attrib[] GetMinAttribs() {
      Attrib[] minAttribs = new Attrib[(int)AttribType.LENGTH];
      foreach (var kv in minAttribDict) {
        minAttribs[(int)kv.Key] = kv.Value;
      }
      return minAttribs;
    }

    public Attrib[] GetMaxAttribs() {
      Attrib[] maxAttribs = new Attrib[(int)AttribType.LENGTH];
      foreach (var kv in maxAttribDict) {
        maxAttribs[(int)kv.Key] = kv.Value;
      }
      return maxAttribs;
    }
  }

  /// <summary>
  /// 数值代表排序的优先级，值越小越靠前
  /// </summary>
  public enum TaskState {
    /// <summary>
    /// 不会参与排序
    /// </summary>
    INACTIVE = 0,
    COMPLETED = 1,
    INPROCESS = 2,
    FINAL = 3
  }

  public enum TaskType {
    [Description("daily")]
    DAILY,
    [Description("main")]
    MAIN
  }

  public enum RequestType {
    ITEM_NUM,
    TASK,
    LV_UP_TO
  }

  public enum Method {
    HAD, CONSUME, ADD
  }

  public struct TaskTemplate {
    public struct TaskRewards {
      public ItemData[] items;
    }

    public string id;
    public string title;
    public TaskType type;
    public string description;
    public string activeTaskId;
    public TaskType activeTaskType;
    public TaskRewards taskRewards;
  }
  #endregion

  #region Data
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
    public long ticks;
    public string title;
    public string content;
    public bool isReceived;
    public string signature;
    public List<ItemData> attachment = new List<ItemData>();

    public MailData(string title, string content, string signature, params ItemData[] attachment) {
      this.title = title;
      this.content = content;
      this.signature = signature;
      if (attachment != null) {
        this.attachment.AddRange(attachment);
      }
      isReceived = this.attachment.Count == 0;
      ticks = DateTime.Now.Ticks;
    }
  }

  [Serializable]
  public class CardData {
    public int guid;
    public string id;
    public int phase;
    public long ticks;

    public CardData(string id, int phase) {
      this.id = id;
      this.phase = phase;
      ticks = DateTime.Now.Ticks;
    }
  }

  [Serializable]
  public class UnitData {
    public string id;
    public int level;
    public long ticks;
    public int passiveSkillLevels;

    public UnitData(string id, int level, int passiveSkillLevels) {
      this.id = id;
      this.level = level;
      this.passiveSkillLevels = passiveSkillLevels;
      ticks = DateTime.Now.Ticks;
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
    public List<int> cardIdList = new List<int>();
  }

  [Serializable]
  public class UserData {
    public int lv;
    public int ap;
    public int exp;
    public int maxAp;
    public string name;
    public int validGuid;
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
      if(ResourceManager.TryGetMaxAp(lv, out int ret)) {
        ap = maxAp = ret;
      } else {
        ap = maxAp = 30;
      }

      tasks = DataManager.GetDefaultTasks();

      for (int i = 0; i < Constant.MAX_TEAN_COUNT; i++) {
        teams.Add(new TeamData());
      }
      teams[0].isSelected = true;

#if TEST
      if (string.IsNullOrEmpty(name)) {
        this.name = "游客";
      }
#endif
    }
  }
  #endregion
}