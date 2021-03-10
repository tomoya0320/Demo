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
    public string groupId;
    public string name;
    public bool hideDisplay;
    public string iconFileName;
    [TextArea]
    public string description;
    public Blackboard blackboard;
  }

  public enum CardType {
    [Description("攻击")]
    ATTACK,
    [Description("技能")]
    SKILL,
    [Description("诅咒")]
    CURSE
  }

  public struct CardAnimEvent {
    public float waitingTime;
    public bool loadFromBlackboard;
    [ShowIf("@!this.loadFromBlackboard")]
    public int eventCount;
  }

  public struct CardPhase {
    public string name;
    public bool consume;
    [TextArea]
    public string description;
    public string cardAnimName;
    public string iconFileName;
    public int energyConsumption;
    public CardAnimEvent cardAnimEvent;
    public BuffTemplate[] buffTemplates;
    public ItemData[] updateRequestItems;
  }

  public struct CardTemplate {
    public string id;
    /// <summary>
    /// 该卡牌是否只对自己生效
    /// 主要影响candidate的选择
    /// </summary>
    public bool onlySelf;
    public CardType cardType;
    public CardPhase[] cardPhases;
  }

  public struct PassiveSkillPhase {
    public string name;
    [TextArea]
    public string description;
    public string iconFileName;
    public BuffTemplate[] buffTemplates;
    public ItemData[] updateRequestItems;
  }

  /// <summary>
  /// 绝对阵营
  /// </summary>
  [Flags]
  public enum Camp {
    NONE = 0,
    ALLY = 1 << 0,
    ENEMY = 1 << 1,
    ALL = ALLY | ENEMY,
  }

  public struct UnitTemplate {
    public string id;
    public Camp camp;
    public string tag;
    public string name;
    public int maxLevel;
    public string prefabName;
    [TextArea]
    public string description;
    // 头像
    public string headFileName;
    // 半身像
    public string portraitFileName;
    // 立绘
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
    [Description("achievement")]
    ACHIEVEMENT,
    [Description("main_story")]
    MAIN_STORY,
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

  public struct Rewards {
    public ItemData[] items;
  }

  public struct TaskTemplate {
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
    // 任务默认的状态
    public TaskState defaultState;
    [TextArea]
    public string description;
    public string activeTaskId;
    public Rewards taskRewards;
    public RequestTemplate taskRequest;
  }

  public struct MailTemplate {
    public string id;
    public string title;
    public string content;
    public string signature;
    public ItemData[] attachments;
  }

  public enum UIType {
    NORMAL,
    TOP,
    TIP
  }

  public struct UITemplate {
    public string name;
    public UIType uiType;
    public string typeName;
    public string prefabName;
    public float cacheTime;
  }

  public enum LevelType {
    /// <summary>
    /// 主线关卡
    /// </summary>
    MAIN_STORY,
    /// <summary>
    /// 教程关卡
    /// </summary>
    GUIDE,
  }

  public struct UnitArgs {
    public string id;
    public int lv;
    public int passiveSkillLv;

    public static UnitArgs Create(UnitData unitData) {
      UnitArgs unitArgs = new UnitArgs {
        id = unitData.id,
        lv = unitData.lv,
        passiveSkillLv = unitData.passiveSkillLv
      };
      return unitArgs;
    }
  }

  public struct CardArgs {
    public string id;
    public int phase;

    public static CardArgs Create(CardData cardData) {
      CardArgs cardArgs = new CardArgs {
        id = cardData.id,
        phase = cardData.phase
      };
      return cardArgs;
    }
  }

  public struct TeamArgs {
    public UnitArgs unitArgs;
    public CardArgs[] cardArgsArray;

    public TeamArgs(UnitArgs unitArgs, CardArgs[] cardArgsArray) {
      this.unitArgs = unitArgs;
      this.cardArgsArray = cardArgsArray;
    }
  }
  
  public struct LevelTemplate {
    public string id;
    public string bgm;
    public string title;
    public LevelType type;
    public string background;
    [TextArea]
    public string description;
    public string beforeAVGId;
    public string afterAVGId;
    public string preLevelId;
    public Rewards regularRewards;
    public Rewards firstCompletedRewards;
    public TeamArgs[] enemyTeams;
    public Blackboard blackboard;
    public BuffTemplate[] buffTemplates;
    public bool isCustomTeam;
    [ShowIf("isCustomTeam")]
    public TeamArgs customTeam;
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
    /// <summary>
    /// 在死亡前
    /// </summary>
    ON_BEFORE_DIE,
  }

  public struct Behavior {
    public string behaviorTreeId;
    public BehaviorTime behaviorTime;
  }

  public enum StackType {
    /// <summary>
    /// 不叠加即后面加的相同buff都无效
    /// </summary>
    NONE,
    /// <summary>
    /// 默认叠加方式相同的buff都有独立的生命周期
    /// </summary>
    DEFAULT,
  }

  public struct BuffTemplate {
    public string id;
    public StackType stackType;
    public TargetSelector selector;
    public Behavior[] behaviors;
    public Blackboard blackboard;
  }

  #endregion

  #region Data

  [Serializable]
  public class ItemData {
    public string id;
    public int count;

    private ItemData() { }

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
    public string id;
    public long ticks;
    public bool isReceived;

    private MailData() { }

    public MailData(string id) {
      this.id = id;
      ticks = DateTime.Now.Ticks;
      isReceived = ResourceManager.Load<MailTemplate>(id).attachments.Length == 0;
    }
  }

  [Serializable]
  public class CardData {
    [HideInInspector]
    public int guid;
    public string id;
    public int phase;

    private CardData() { }

    public CardData(string id, int phase) {
      this.id = id;
      this.phase = phase;
    }

    public CardData(int guid, string id, int phase) {
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
    public int lv;
    public int exp;
    public int passiveSkillLv;

    private UnitData() { }

    public UnitData(string id, int lv, int passiveSkillLv) {
      this.id = id;
      this.lv = lv;
      this.passiveSkillLv = passiveSkillLv;
    }
  }


  [Serializable]
  public class TaskData {
    public string id;
    public int process;
    public TaskState state;

    private TaskData() { }

    public TaskData(string id, TaskState state) {
      this.id = id;
      this.state = state;
    }
  }

  [Serializable]
  public class TeamData {
    public string charId;
    public string name = "默认队伍名";
    public List<int> cardGuids = new List<int>();
  }

  [Serializable]
  public class PlayerData {
    public int lv;
    public string name;
    public int validGuid;
    public int teamIndex;
  }

  [Serializable]
  public class UserDB {
    public PlayerData player = new PlayerData();
    public List<MailData> mails = new List<MailData>();
    public List<ItemData> items = new List<ItemData>();
    public List<CardData> cards = new List<CardData>();
    public List<UnitData> chars = new List<UnitData>();
    public List<TeamData> teams = new List<TeamData>();
    public List<TaskData> tasks = new List<TaskData>();
    public List<LevelData> levels = new List<LevelData>();

    public static UserDB Create() {
      UserDB userDB = new UserDB();

      foreach (var taskTemplate in ResourceManager.LoadAll<TaskTemplate>()) {
        userDB.tasks.Add(new TaskData(taskTemplate.id, taskTemplate.defaultState));
      }

      foreach (var levelTemplate in ResourceManager.LoadAll<LevelTemplate>()) {
        userDB.levels.Add(new LevelData(levelTemplate.id));
      }

      for (int i = 0; i < Constant.MAX_TEAN_COUNT; i++) {
        userDB.teams.Add(new TeamData());
      }
#if BETA
      userDB.chars.Add(new UnitData("char_001", 0, 0));
      userDB.cards.Add(new CardData(userDB.player.validGuid++, "card_001", 0));
      userDB.cards.Add(new CardData(userDB.player.validGuid++, "card_001", 0));
      userDB.cards.Add(new CardData(userDB.player.validGuid++, "card_001", 1));
      userDB.cards.Add(new CardData(userDB.player.validGuid++, "card_001", 1));
      userDB.cards.Add(new CardData(userDB.player.validGuid++, "card_002", 0));
      userDB.cards.Add(new CardData(userDB.player.validGuid++, "card_002", 0));
      userDB.cards.Add(new CardData(userDB.player.validGuid++, "card_002", 1));
      userDB.cards.Add(new CardData(userDB.player.validGuid++, "card_002", 1));
      userDB.cards.Add(new CardData(userDB.player.validGuid++, "card_003", 0));
      userDB.cards.Add(new CardData(userDB.player.validGuid++, "card_003", 0));
      userDB.cards.Add(new CardData(userDB.player.validGuid++, "card_004", 0));
      userDB.cards.Add(new CardData(userDB.player.validGuid++, "card_004", 0));
      userDB.mails.Add(new MailData("test_mail_1"));
#endif
      return userDB;
    }
  }

  [Serializable]
  public class LevelData {
    public string id;
    [HideInInspector]
    public bool completed;

    private LevelData() { }

    public LevelData(string id) {
      this.id = id;
    }
  }

  #endregion
}