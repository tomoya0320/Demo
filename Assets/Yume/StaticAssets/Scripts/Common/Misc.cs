using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using Yume.Battle;
using XLua;
using Random = System.Random;
using Axis = UnityEngine.RectTransform.Axis;
using Component = UnityEngine.Component;

namespace Yume {
  #region Template

  public struct ItemArgs {
    public string id;
    public int count;
  }

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
    [ShowIf("@!loadFromBlackboard")]
    public int eventCount;
  }

  public struct CardPhase {
    public string name;
    public bool retain;
    public bool consume;
    [TextArea]
    public string description;
    public string cardAnimName;
    public string iconFileName;
    public int energyConsumption;
    public CardPlayer cardPlayer;
    public CardAnimEvent cardAnimEvent;
    public BuffTemplate[] buffTemplates;
    public ItemArgs[] updateRequestItems;
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
    public ItemArgs[] updateRequestItems;
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

  public struct Rewards {
    public ItemArgs[] items;
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

  public struct TaskTemplate {
    public enum Method {
      HAD,
      CONSUME,
      ADD
    }
    
    public struct Request {
      public Method method;
      /// <summary>
      /// 完成任务后是否消耗所需物品
      /// </summary>
      public bool consumable;
      /// <summary>
      /// 是否忽略需求的ID
      /// </summary>
      public bool ignoreId;
      [ShowIf("@!ignoreId")]
      public string requirementId;
      public RequestType type;
      public int requiredCount;
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
    public Request taskRequest;
  }

  public struct MailTemplate {
    public string id;
    public string title;
    public string content;
    public string signature;
    public ItemArgs[] attachments;
  }

  public enum UIType {
    NORMAL,
    TOP,
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
    /// <summary>
    /// 在出牌前
    /// </summary>
    ON_BEFORE_PLAY_CARD,    
    /// <summary>
    /// 在出牌后
    /// </summary>
    ON_LATE_PLAY_CARD,
    /// <summary>
    /// 瞬发
    /// </summary>
    ON_INSTANT,
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

  public struct BuffDisplay {
    public string icon;
    [TextArea]
    public string description;
  }

  public struct BuffTemplate {
    public string id;
    public StackType stackType;
    public BuffDisplay display;
    public TargetSelector selector;
    public Behavior[] behaviors;
    public Blackboard blackboard;
  }

  public enum PositionType {
    LEFT_TOP,
    LEFT_BOTTOM,
    RIGHT_TOP,
    RIGHT_BOTTOM,
  }
  
  public struct GuideStep {
    public PositionType positionType;
    public bool ignoreClickPos;
    public string uiName;
    public string targetObj;
    public bool isDynamicListObj;
    [ShowIf("isDynamicListObj")]
    public string dynamicListObjId;
    public string iconFileName;
    [TextArea]
    public string description;
  }

  public enum ConditionType {
    FIRST_PLAY,
  }

  public enum GuideState {
    INACTIVE,
    ACTIVE,
    COMPLETED,
  }
  
  public struct GuideTemplate {
    public string id;
    public bool ignoreConditionId;
    [ShowIf("@!ignoreConditionId")]
    public string conditionId;
    public ConditionType conditionType;
    public GuideStep[] guideSteps;
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
  public class LevelData {
    public string id;
    [HideInInspector]
    public bool completed;

    private LevelData() { }

    public LevelData(string id) {
      this.id = id;
    }
  }

  [Serializable]
  public class GuideData {
    public string id;
    public GuideState state;
    
    private GuideData() { }

    public GuideData(string id) {
      this.id = id;
      state = GuideState.INACTIVE;
    }
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
    public List<GuideData> guides = new List<GuideData>();

    public static UserDB Create() {
      UserDB userDB = new UserDB();

      foreach (var taskTemplate in ResourceManager.LoadAll<TaskTemplate>()) {
        userDB.tasks.Add(new TaskData(taskTemplate.id, taskTemplate.defaultState));
      }

      foreach (var levelTemplate in ResourceManager.LoadAll<LevelTemplate>()) {
        userDB.levels.Add(new LevelData(levelTemplate.id));
      }
      
      foreach (var guideTemplate in ResourceManager.LoadAll<GuideTemplate>()) {
        userDB.guides.Add(new GuideData(guideTemplate.id));
      }

      for (int i = 0; i < Constant.MAX_TEAN_COUNT; i++) {
        userDB.teams.Add(new TeamData());
      }

      foreach (var initialMail in ResourceManager.Load<string[]>("initialMails")) {
        userDB.mails.Add(new MailData(initialMail));
      }

      foreach (var initialChar in ResourceManager.Load<UnitArgs[]>("initialChars")) {
        userDB.chars.Add(new UnitData(initialChar.id, initialChar.lv, initialChar.passiveSkillLv));
      }

      foreach (var initialCard in ResourceManager.Load<CardArgs[]>("initialCards")) {
        userDB.cards.Add(new CardData(userDB.player.validGuid++, initialCard.id, initialCard.phase));
      }

      return userDB;
    }
  }

  #endregion

  public static class XLuaUtil {
    private static LuaEnv s_luaEnv = new LuaEnv();

    public static object[] DoString(string luaString) {
      return s_luaEnv.DoString(luaString);
    }
  }

  public static class Constant {
    public const int THOUSAND = 1000;
    public const int MAX_TEAN_COUNT = 5;
    public const int MAX_CARD_COUNT = 12;
    public const int MAX_SOUND_COUNT = 5;
    public const int DRAW_CARD_COUNT = 5;
    public const float ANGLE_DELTA = 1.5f;
    public const float SPACING_SCALE = 0.5f;
    public const float OFFSET_SCALE_Y = 0.02f;
    public static readonly int ATTRIB_LENGTH = Enum.GetValues(typeof(AttribType)).Length;
    public static readonly Vector2 SCREEN_SIZE = new Vector2(Screen.width, Screen.height);
  }

  public static class Colors {
    public static readonly Color DISABLE_BUTTON = Color.white * 0.5f;

    public static Color GetAttribColor(AttribType type) {
      switch (type) {
        case AttribType.HP:
          return Color.green;
        case AttribType.ENERGY:
          return Color.blue;
        default:
          return Color.black;
      }
    }
  }

  public static class TransformExtensions {
    public static void SetAngleZ(this Transform transform, float angle) {
      Vector3 eulerAngles = transform.eulerAngles;
      eulerAngles.z = angle;
      transform.eulerAngles = eulerAngles;
    }
  }

  public static class SelectableExtensions {
    /// <summary>
    /// 自定义colors navigation的默认状态 
    /// </summary>
    public static void SetDefaultState(this Selectable selectable) {
      ColorBlock newColors = selectable.colors;
      newColors.disabledColor = Color.white;
      selectable.colors = newColors;

      Navigation newNavigation = selectable.navigation;
      newNavigation.mode = Navigation.Mode.None;
      selectable.navigation = newNavigation;
    }
  }

  public static class ListExtensions {
    /// <summary>
    /// 模拟队列
    /// </summary>
    /// <param name="list"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T Dequeue<T>(this List<T> list) {
      if (list.Count > 0) {
        T element = list.First();
        list.Remove(element);
        return element;
      }
      return default;
    }
  }
  
  public static class TypeExtensions {
    public static List<string> GetFieldNames(this Type type) {
      FieldInfo[] fieldInfos = type.GetFields();
      List<string> fieldNames = new List<string>(fieldInfos.Length);
      foreach (var fieldInfo in fieldInfos) {
        fieldNames.Add(fieldInfo.Name);
      }
      return fieldNames;
    }

    public static List<string> GetDescriptions(this Type type) {
      List<string> descriptions = new List<string>();
      foreach (Enum enumValue in Enum.GetValues(type)) {
        descriptions.Add(enumValue.GetDescription());
      }
      return descriptions;
    }
  }

  public static class RectTransformExtensions {
    /// <summary>
    /// Set SizeX With CurrentAnchors
    /// </summary>
    /// <param name="rectTransform"></param>
    /// <param name="x"></param>
    public static void SetSizeX(this RectTransform rectTransform, float x) {
      rectTransform.SetSizeWithCurrentAnchors(Axis.Horizontal, x);
    }

    /// <summary>
    /// Set SizeY With CurrentAnchors
    /// </summary>
    /// <param name="rectTransform"></param>
    /// <param name="y"></param>
    public static void SetSizeY(this RectTransform rectTransform, float y) {
      rectTransform.SetSizeWithCurrentAnchors(Axis.Vertical, y);
    }

    public static void Reset(this RectTransform rectTransform) {
      rectTransform.anchoredPosition3D = Vector3.zero;
      rectTransform.localScale = Vector3.one;
      rectTransform.eulerAngles = Vector3.zero;
    }
  }

  public static class DictionaryExtensions {
    /// <summary>
    /// 兼容了key为null的情况
    /// 如果key为null则value为default并返回false
    /// </summary>
    /// <param name="dictionary"></param>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <returns></returns>
    public static bool TryGetValueEx<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, out TValue value) {
      if (typeof(TKey).IsClass && key == null) {
        value = default;
        return false;
      } else {
        return dictionary.TryGetValue(key, out value);
      }
    }
  }

  public static class GraphicExtensions {
    public static void SetAlpha(this Graphic graphic, float alpha) {
      Color color = graphic.color;
      color.a = alpha;
      graphic.color = color;
    }
  }

  public static class ImageExtensions {
    public static async void SetSpriteAsync(this Image image, string name) {
      image.sprite = await Addressables.LoadAssetAsync<Sprite>(name).Task;
    }
  }

  public static class GameObjectExtensions {
    public static RectTransform GetRectTransform(this GameObject gameObject) {
      return gameObject.GetComponent<RectTransform>();
    }

    public static void SetParent(this GameObject gameObject, Transform parent) {
      gameObject.transform.SetParent(parent);
    }

    /// <summary>
    /// 只有物体的激活状态与value不同时才会调用
    /// </summary>
    /// <param name="gameObject"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool SetActiveEx(this GameObject gameObject, bool value) {
      if (gameObject.activeSelf != value) {
        gameObject.SetActive(value);
        return true;
      }
      return false;
    }

    public static Transform GetParent(this GameObject gameObject) {
      return gameObject.transform.parent;
    }

    /// <summary>
    /// 如果找不到组件就添加一个
    /// </summary>
    /// <param name="gameObject"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T GetComponentEx<T>(this GameObject gameObject) where T : Component {
      T component = gameObject.GetComponent<T>();
      if (component == null) {
        component = gameObject.AddComponent<T>();
      }
      return component;
    }
  }

  public static class EnumExtensions {
    /// <summary>
    /// 获取当前枚举描述
    /// </summary>
    /// <param name="enumValue"></param>
    /// <returns></returns>
    public static string GetDescription(this Enum enumValue) {
      Type type = enumValue.GetType();
      MemberInfo[] memInfo = type.GetMember(enumValue.ToString());
      if (memInfo != null && memInfo.Length > 0) {
        object[] attrs = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
        if (attrs != null && attrs.Length > 0)
          return ((DescriptionAttribute)attrs[0]).Description;
      }
      return enumValue.ToString();
    }
  }

  public static class AssemblyUtil {
    private static Assembly s_assembly = Assembly.GetExecutingAssembly();

    public static object CreateInstance(string typeName) {
      return s_assembly.CreateInstance(typeName);
    }
  }

  public static class AnimatorParams {
    public const string HURT = "hurt";
  }

  public static class StringMap {
    #region datetime

    public const string TODAY = "今天";
    public const string SOME_DAY_BEFORE = "{0}天前";
    public const string SOME_WEEK_BEFORE = "{0}周前";
    public const string SOME_MONTH_BEFORE = "{0}月前";
    public const string SOME_YEAR_BEFORE = "{0}年前";

    #endregion

    public const string EMPTY = "";
    public const string SWITCH = "Switch";
    public const string VALUE = "Value";
    public const string TASK_COMPLETED = "任务[{0}]已完成！";
    public const string QUIT_GAME_TIP = "确认退出游戏？";
    public const string QUIT = "退出";
    public const string CANCEL = "取消";
    public const string RETRY = "重试";
    public const string RETURN_TO_LOGIN = "返回登录界面";
    public const string COMPLETED = "已完成";
    public const string CLICK_GET = "点击领取";
    public const string GOT = "已领取";
    public const string GET = "获得";
    public const string DEFAULT_BGM = "SOTE_Shrine_v2";
    public const string DEFAULT_HURT_SOUND = "SOTE_SFX_BlockAtk_v2";
    public const string DEFAULT_CLICK_SOUND = "点击2";
    public const string EXP_ITEM_ID = "item_0_exp";
    public const string DIAMOND_ITEM_ID = "item_1_diamond";
    public const string GOLD_ITEM_ID = "item_2_gold";
    public const string LEVEL_UP_TO = "等级提升至Lv{0}";
    public const string UI_RED_DOT = "red dot";
    public const string CHAR_EXP = "char_exp";
    public const string CONSUME_GOLD = "消耗金币 {0}";
  }

  public static class StringUtil {
    /// <summary>
    /// 以万、亿等单位来显示数字并忽略小数部分，避免数字过长导致显示问题
    /// </summary>
    /// <param name="num"></param>
    /// <returns></returns>
    public static string IntegerToString(int num) {
      if (num < 10000) {
        return num.ToString();
      } else if (num < 100000000) {
        return (num / 10000) + "万";
      } else {
        return (num / 100000000) + "亿";
      }
    }

    public static string StringToBase64(string str) {
      byte[] bytes = Encoding.UTF8.GetBytes(str);
      return Convert.ToBase64String(bytes);
    }

    public static string Base64ToString(string str) {
      byte[] bytes = Convert.FromBase64String(str);
      return Encoding.UTF8.GetString(bytes);
    }

    /// <summary>
    /// 将卡牌描述里类似{xxx}的字符串替换成buff的blackboard里的实际数值
    /// 比如有个描述是{0:atkRate%}即为取第一buff的blackboard里key为atkRate的值并以百分比的形式显示(atkRate需为float类型)
    /// 如果是附加xxx属性的话格式则为{0:attribAdditive:HP#value}
    /// </summary>
    public static string GetActualDescription(string description, BuffTemplate[] buffTemplates) {
      StringBuilder result = new StringBuilder();
      int start = 0;
      for (int i = 0; i < description.Length; i++) {
        switch (description[i]) {
          case '{': {
            result.Append(description.Substring(start, i - start));
            start = i + 1;
            break;
          }
          case '}': {
            List<string> expression = description.Substring(start, i - start).Split(':').ToList();
            var blackboard = buffTemplates[int.Parse(expression[0])].blackboard;
            int nextIndex = 1;
            if (expression[nextIndex] == Blackboard.BUFFS) {
              blackboard = blackboard.GetValue<BuffTemplates>(Blackboard.BUFFS)[int.Parse(expression[nextIndex + 1])].blackboard;
              nextIndex += 2;
            }
            switch (expression[nextIndex]) {
              case Blackboard.ATTRIB_ADDITIVES:
                var attribAdditives = blackboard.GetValue<AttribAdditives>(Blackboard.ATTRIB_ADDITIVES);
                string[] kv = expression[nextIndex + 1].Split('#');
                var attribAdditive = attribAdditives.Find(a => a.type.ToString() == kv[0]);
                result.Append(_GetValueFromType(kv[1], attribAdditive));
                break;
              case Blackboard.CARD_SELECTOR:
                var cardSelector = blackboard.GetValue<CardSelector>(Blackboard.CARD_SELECTOR);
                result.Append(_GetValueFromType(expression[nextIndex + 1], cardSelector));
                break;
              default:
                result.Append(_GetValueFromBlackboard(expression.Last(), blackboard));
                break;
            }
            start = i + 1;
            break;
          }
        }
      }
      if (start < description.Length) {
        result.Append(description.Substring(start));
      }
      return result.ToString();
    }

    public static string GetActualDescription(string description, Blackboard blackboard) {
      StringBuilder result = new StringBuilder();
      int start = 0;
      for (int i = 0; i < description.Length; i++) {
        switch (description[i]) {
          case '{': {
            result.Append(description.Substring(start, i - start));
            start = i + 1;
            break;
          }
          case '}': {
            string[] key = description.Substring(start, i - start).Split(':');
            switch (key[0]) {
              case Blackboard.ATTRIB_ADDITIVES:
                var attribAdditives = blackboard.GetValue<AttribAdditives>(Blackboard.ATTRIB_ADDITIVES);
                string[] kv = key[1].Split('#');
                var attribAdditive = attribAdditives.Find(a => a.type.ToString() == kv[0]);
                result.Append(_GetValueFromType(kv[1], attribAdditive));
                break;
              case Blackboard.CARD_SELECTOR:
                var cardSelector = blackboard.GetValue<CardSelector>(Blackboard.CARD_SELECTOR);
                result.Append(_GetValueFromType(key[1], cardSelector));
                break;
              default:
                result.Append(_GetValueFromBlackboard(key[0], blackboard));
                break;
            }
            start = i + 1;
            break;
          }
        }
      }
      if (start < description.Length) {
        result.Append(description.Substring(start));
      }
      return result.ToString();
    }

    private static string _GetValueFromBlackboard(string key, Blackboard blackboard) {
      if (key[key.Length - 1] == '%') {
        key = key.Substring(0, key.Length - 1);
        object value = blackboard.GetValue<object>(key);
        return value switch {
          int i => $"{i * 100 / Constant.THOUSAND}%",
          float f => $"{f * 100.0f}%",
          _ => $"{value}"
        };
      } else {
        return $"{blackboard.GetValue<object>(key)}";
      }
    }

    private static string _GetValueFromType(string key, object obj) {
      if (key[key.Length - 1] == '%') {
        key = key.Substring(0, key.Length - 1);
        object value = obj.GetType().GetField(key).GetValue(obj);
        return value switch {
          int i => $"{i * 100 / Constant.THOUSAND}%",
          float f => $"{f * 100.0f}%",
          _ => $"{value}"
        };
      } else {
        return $"{obj.GetType().GetField(key).GetValue(obj)}";
      }
    }

    public static string GetStringByBattleResult(BattleResult result) {
      switch (result) {
        case BattleResult.WIN:
          return "作战胜利";
        case BattleResult.LOSE:
          return "作战失败";
        case BattleResult.ABORT:
          return "放弃作战";
        default:
          return "战斗结果为空！";
      }
    }
  }

  public static class ScreenUtil {
    public static Vector2 mousePos => Input.mousePosition;
    public static Vector2 mousePosNormalized => mousePos / Constant.SCREEN_SIZE;
  }

  public static class MathUtil {
    public static readonly Random random = new Random();

    /*
    /// <summary>
    /// 在XOZ平面上旋转一个Vector3
    /// </summary>
    public static Vector3 RotateVector3(Vector3 v3, float angle) {
      angle = -angle;
      float x = v3.x * Mathf.Cos(Mathf.Deg2Rad * angle) - v3.z * Mathf.Sin(Mathf.Deg2Rad * angle);
      float z = v3.x * Mathf.Sin(Mathf.Deg2Rad * angle) + v3.z * Mathf.Cos(Mathf.Deg2Rad * angle);
      return new Vector3(x, 0, z);
    }
  
    /// <summary>
    /// 计算两个向量之间的夹角，带符号,n为旋转轴
    /// </summary>
    public static float AngleSigned(Vector3 v1, Vector3 v2, Vector3 n) {
      return Mathf.Atan2(
          Vector3.Dot(n, Vector3.Cross(v1, v2)),
          Vector3.Dot(v1, v2)) * Mathf.Rad2Deg;
    }
    */

    /// <summary>
    /// 计算给定的时期（不超过现在的时间）到现在过去了多久
    /// </summary>
    public static string CalDateDeltaFromNow(DateTime dateTime) {
      TimeSpan timeSpan = DateTime.Now - dateTime;
      if (timeSpan.Days == 0) {
        return StringMap.TODAY;
      } else if (timeSpan.Days < 7) {
        return string.Format(StringMap.SOME_DAY_BEFORE, timeSpan.Days);
      } else if (timeSpan.Days < 30) {
        return string.Format(StringMap.SOME_WEEK_BEFORE, timeSpan.Days / 7);
      } else if (timeSpan.Days < 365) {
        return string.Format(StringMap.SOME_MONTH_BEFORE, timeSpan.Days / 30);
      } else {
        return string.Format(StringMap.SOME_YEAR_BEFORE, timeSpan.Days / 365);
      }
    }

    /// <summary>
    /// 计算给定的时期（不超过现在的时间）到现在过去了多久
    /// </summary>
    public static string CalDateDeltaFromNow(long dateData) {
      DateTime dateTime = DateTime.FromBinary(dateData);
      return CalDateDeltaFromNow(dateTime);
    }

    /// <summary>
    /// 计算玩家当前等级还需要多少经验才能升级
    /// </summary>
    public static int CalNeedUserExp(int lv) {
      return 50 + lv * 50;
    }

    /// <summary>
    /// 计算角色当前等级还需要多少经验才能升级
    /// </summary>
    public static int CalNeedCharExp(int lv) {
      return 100 + lv * 300;
    }

    /// <summary>
    /// 根据角色经验值计算需要花费多少金币
    /// </summary>
    /// <param name="exp"></param>
    /// <returns></returns>
    public static int CalNeedGoldByCharExp(int exp) {
      return 10 * exp;
    }

    public static Attrib[] CalculateAttribs(string id, int level) {
      UnitTemplate unitTemplate = ResourceManager.Load<UnitTemplate>(id);
      Attrib[] minAttribs = unitTemplate.GetMinAttribs();
      Attrib[] maxAttribs = unitTemplate.GetMaxAttribs();

      if (minAttribs.Length != maxAttribs.Length) {
        throw new Exception("最小属性数组和最大属性数组长度不一致！");
      }

      float p = (float)level / unitTemplate.maxLevel;
      Attrib[] attribs = new Attrib[minAttribs.Length];
      AnimationCurve curve;
      if (ResourceManager.Contains<AnimationCurve>(id)) {
        curve = ResourceManager.Load<AnimationCurve>(id);
      } else {
        curve = ResourceManager.Load<AnimationCurve>("common_curve");
      }
      for (int i = 0; i < attribs.Length; i++) {
        attribs[i] = minAttribs[i] + (maxAttribs[i] - minAttribs[i]) * curve.Evaluate(p);
      }

      return attribs;
    }

    /// <summary>
    /// 洗牌算法
    /// </summary>
    /// <param name="array"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T[] FisherYatesShuffle<T>(T[] array) {
      for (int i = array.Length - 1; i > 0; i--) {
        int index = random.Next(0, i);
        T temp = array[i];
        array[i] = array[index];
        array[index] = temp;
      }
      return array;
    }

    public static bool Prob(float prob) {
      return random.NextDouble() < prob;
    }

    public static bool IsPointInRect(Vector2 point, Vector4 rect) {
      return point.x >= rect.x && point.y >= rect.y && point.x <= rect.z && point.y <= rect.w;
    }
  }

  public abstract class SingletonMono<T> : SerializedMonoBehaviour where T : SingletonMono<T> {
    private static T s_instance;

    public static T instance {
      get {
        if (s_instance == null) {
          GameObject obj = new GameObject(typeof(T).Name);
          s_instance = obj.AddComponent<T>();
        }
        return s_instance;
      }
    }
    public static bool hasInstance => s_instance != null;

    private void Awake() {
      if (!hasInstance) {
        s_instance = this as T;
        OnInit();
      } else {
        Debug.LogWarning("Duplicate Instance:" + nameof(T));
        DestroyImmediate(gameObject);
      }
    }

    protected virtual void OnInit() { }
  }

  public class YEnumerator<T> {
    private T[] m_array;
    private int m_index;
    public T current => (m_index >= 0 && m_index < m_array.Length) ? m_array[m_index] : default;

    public YEnumerator(T[] array) {
      m_array = array ?? new T[0];
      m_index = -1;
    }

    public bool TryMoveNext() {
      if (m_index + 1 < m_array.Length) {
        m_index++;
        return true;
      }
      return false;
    }
  }
}