using System.Collections.Generic;
using System.ComponentModel;
using Yume.Battle;
using Yume.Data;

namespace Yume.Prototype {
  public struct ItemPrototype {
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

  public struct CardPrototype {
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

  public struct UnitPrototype {
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

  public struct TaskPrototype {
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
}