using System;
using System.Collections.Generic;
using System.ComponentModel;
using Sirenix.OdinInspector;
using UnityEngine;
using Yume.Battle;
using Yume.Resource;

namespace Yume.Data {
  #region Template

  public struct ItemTemplate {
    public string id;
    public string name;
    public bool hideDisplay;
    public string iconFileName;
    [TextArea]
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
    [TextArea]
    public string description;
    public string iconFileName;
    public int energyConsumption;
    public string buffTemplateId;
    public Blackboard blackboard;
  }

  public struct CardTemplate {
    public string id;
    public CardType cardType;
    public CardPhase[] cardPhases;
  }

  public struct PassiveSkillPhase {
    public string name;
    [TextArea]
    public string description;
    public string iconFileName;
    public string buffTemplateId;
    public Blackboard blackboard;
  }

  public struct UnitTemplate {
    public string id;
    public string tag;
    public string name;
    public int maxLevel;
    public string prefabName;
    [TextArea]
    public string description;
    public string portraitFileName;
    public string illustrationFileName;
    public PassiveSkillPhase[] passiveSkillPhases;
    public Dictionary<AttribType, Attrib> minAttribDict;
    public Dictionary<AttribType, Attrib> maxAttribDict;

    public Attrib[] GetMinAttribs() {
      Attrib[] minAttribs = new Attrib[Constant.ATTRIB_LENGTH];
      foreach (var kv in minAttribDict) {
        minAttribs[(int)kv.Key] = kv.Value;
      }
      return minAttribs;
    }

    public Attrib[] GetMaxAttribs() {
      Attrib[] maxAttribs = new Attrib[Constant.ATTRIB_LENGTH];
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
    COMPLETED = 0,
    INPROCESS = 1,
    FINAL = 2,
    /// <summary>
    /// 并不会显示
    /// </summary>
    INACTIVE = 3
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
    HAD,
    CONSUME,
    ADD
  }

  [Serializable]
  public struct Rewards {
    public ItemData[] items;
  }

  [Serializable]
  public struct TaskTemplate {
    [Serializable]
    public struct RequestTemplate {
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
      public int requiredCount;
      public string requirementId;
      [TextArea]
      public string description;
    }

    public string id;
    public string title;
    public TaskType type;
    [TextArea]
    public string description;
    public string activeTaskId;
    public Rewards taskRewards;
    public RequestTemplate taskRequest;
  }

  public enum UIType {
    NORMAL,
    TOP,
    TIP
  }

  [Serializable]
  public struct UITemplate {
    public string name;
    public UIType uiType;
    public string typeName;
    public string prefabName;
    public float cacheTime;
  }

  [Serializable]
  public struct EnemyTemplate {
    public UnitData enemy;
    public CardData[] cards;
  }

  [Serializable]
  public struct LevelTemplate {
    public string id;
    public string bgm;
    public string title;
    public string background;
    [TextArea]
    public string description;
    public string iconFileName;
    public string beforeAVGId;
    public string afterAVGId;
    public string preLevelId;
    public Rewards regularRewards;
    public Rewards firstCompletedRewards;
    public EnemyTemplate[] enemyTemplates;
  }

  public enum BehaviorTime {
    /// <summary>
    /// 每回合开始前
    /// </summary>
    ON_BEFORE_TURN,
    /// <summary>
    /// 每回合结束后
    /// </summary>
    ON_LATE_TURN,
    /// <summary>
    /// 当行为被添加时
    /// </summary>
    ON_ADD,
    /// <summary>
    /// 当行为被移除时
    /// </summary>
    ON_REMOVE,
    /// <summary>
    /// 在造成伤害前
    /// </summary>
    ON_BEFORE_DAMAGE,
    /// <summary>
    /// 在造成伤害后
    /// </summary> 
    ON_LATE_DAMAGE,  
    /// <summary>
    /// 在被造成伤害前
    /// </summary>
    ON_BEFORE_DAMAGED,
    /// <summary>
    /// 在被造成伤害后
    /// </summary> 
    ON_LATE_DAMAGED,
  }

  [Serializable]
  public struct Behavior {
    public string behaviorTreeId;
    public BehaviorTime behaviorTime;
  }

  [Serializable]
  public struct BuffTemplate {
    public string id;
    public bool stackable;
    public Behavior[] behaviors;
  }

  #endregion

  #region Data

  [Serializable]
  public class ItemData {
    public string id;
    public int count;

    public ItemData(string id, int count) {
      this.id = id;
      this.count = count;
    }

    public ItemData Clone() {
      return new ItemData(id, count);
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
    [HideInInspector]
    public int guid;
    public string id;
    public int phase;

    public CardData(string id, int phase) {
      this.id = id;
      this.phase = phase;
    }

    private CardData(int guid, string id, int phase) {
      this.guid = guid;
      this.id = id;
      this.phase = phase;
    }

    public CardData Clone() {
      return new CardData(guid, id, phase);
    }
  }

  [Serializable]
  public class UnitData {
    public string id;
    public int level;
    public int passiveSkillLevel;

    public UnitData(string id, int level, int passiveSkillLevel) {
      this.id = id;
      this.level = level;
      this.passiveSkillLevel = passiveSkillLevel;
    }
  }

  [Serializable]
  public class TaskData {
    public string id;
    [HideInInspector]
    public int process;
    public TaskState state;
  }

  [Serializable]
  public class TeamData {
    public string charId;
    public string name = "默认队伍名";
    public List<int> cardGuidList = new List<int>();
  }

  [Serializable]
  public class UserData {
    public int lv;
    public int ap;
    public int maxAp;
    public string name;
    public int validGuid;
    public int teamIndex;
    public string account;
    public string password;
    public List<MailData> mails = new List<MailData>();
    public List<ItemData> items = new List<ItemData>();
    public List<CardData> cards = new List<CardData>();
    public List<UnitData> chars = new List<UnitData>();
    public List<TeamData> teams = new List<TeamData>();
    public List<TaskData> tasks = new List<TaskData>();
    public List<LevelData> levels = new List<LevelData>();

    public UserData(string name = "", string account = "", string password = "") {
      this.name = name;
      this.account = account;
      this.password = password;

      ap = maxAp = ResourceManager.Load<int>($"{lv}");

      tasks.AddRange(ResourceManager.LoadAll<TaskData>());
      levels.AddRange(ResourceManager.LoadAll<LevelData>());

      for (int i = 0; i < Constant.MAX_TEAN_COUNT; i++) {
        teams.Add(new TeamData());
      }

#if TEST
      if (string.IsNullOrEmpty(name)) {
        this.name = "游客";
      }
#endif
    }
  }

  [Serializable]
  public class LevelData {
    public string id;
    [HideInInspector]
    public bool completed;
  }

  #endregion
}