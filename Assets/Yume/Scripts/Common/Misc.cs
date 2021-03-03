using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Yume.Battle;
using Yume.Resource;
using XLua;
using Yume.Data;
using Random = System.Random;
using Axis = UnityEngine.RectTransform.Axis;
using Component = UnityEngine.Component;

public static class XLuaUtil {
  private static LuaEnv s_luaEnv = new LuaEnv();

  public static object[] DoString(string luaString) {
    return s_luaEnv.DoString(luaString);
  }
}

public static class Constant {
  public const int MAX_TEAN_COUNT = 5;
  public const int MAX_CARD_COUNT = 12;
  public const int MAX_SOUND_COUNT = 5;
  public const int HAND_CARD_THRESHOLD = 6;
  // ReSharper disable once InconsistentNaming
  public static readonly int ATTRIB_LENGTH = Enum.GetValues(typeof(AttribType)).Length;
  // ReSharper disable once InconsistentNaming
  public static readonly Vector2 SCREEN_SIZE = new Vector2(Screen.width, Screen.height);
}

public static class Colors {
  // ReSharper disable once InconsistentNaming
  public static readonly Color DISABLE_BUTTON = Color.white * 0.5f;
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

public static class ListExtensions {
  public static T First<T>(this List<T> list) {
    return list.Count == 0 ? default : list[0];
  }

  public static T Last<T>(this List<T> list) {
    return list.Count == 0 ? default : list[list.Count - 1];
  }

  public static void RemoveRange<T>(this List<T> list, IEnumerable<T> items) {
    foreach (var item in items) {
      list.Remove(item);
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

  /// <summary>
  /// 深度优先遍历查找所有的子物体
  /// </summary>
  /// <param name="gameObject"></param>
  /// <param name="name"></param>
  /// <returns></returns>
  public static GameObject FindGo(this GameObject gameObject, string name) {
    Stack<Transform> stack = new Stack<Transform>();
    stack.Push(gameObject.transform);
    while (stack.Count > 0) {
      var tempTransform = stack.Pop();
      if (tempTransform.name == name) {
        return tempTransform.gameObject;
      }
      for (int i = 0; i < tempTransform.childCount; i++) {
        stack.Push(tempTransform.GetChild(i));
      }
    }

    return null;
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
  public const string LOGIN = "login";
  public const string HOME = "home";
  public const string DEFAULT_BGM = "SOTE_Shrine_v2";
  public const string DEFAULT_HURT_SOUND = "SOTE_SFX_BlockAtk_v2";
  public const string DEFAULT_CLICK_SOUND = "点击2";
  public const string EXP_ID = "item_0_exp";
  public const string DIAMOND_ID = "item_1_diamond";
  public const string GOLD_ID = "item_2_gold";
  public const string LEVEL_UP_TO = "等级提升至Lv{0}";
  public const string AVG = "avg";
  public const string BG = "bg";
  public const string NAME = "name";
  public const string DIALOGUE = "dialogue";
  public const string UI_RED_POINT = "red point";
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
/*
  /// <summary>
  /// 将描述里类似{xxx}的字符串替换成blackboard里的实际数值
  /// </summary>
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
          string key = description.Substring(start, i - start);
          if (key[key.Length - 1] == '%') {
            key = key.Substring(0, key.Length - 1);
            object value = blackboard.GetValue<object>(key);
            if (value is float f) {
              result.Append($"{f * 100.0f}%");
            } else {
              Debug.LogWarning($"非float类型！type:{value.GetType()}");
              result.Append(value);
            }
          } else {
            result.Append(blackboard.GetValue<object>(key));
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
*/
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
  /// 计算当前等级还需要多少经验才能升级
  /// </summary>
  public static int CalNeededExp(int lv) {
    return 50 + lv * 50;
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
    var curve = ResourceManager.Load<AnimationCurve>(id);
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

public class YTween {
  public int count { private set; get; }

  public void DOLocalMove(Transform transform, Vector3 pos, float duration, Action onCompleted = null) {
    count++;
    transform.DOLocalMove(pos, duration).OnComplete(
      () => {
        count--;
        onCompleted?.Invoke();
      }
    );
  }

  public void DOScale(Transform transform, Vector3 scale, float duration, Action onCompleted = null) {
    count++;
    transform.DOScale(scale, duration).OnComplete(
      () => {
        count--;
        onCompleted?.Invoke();
      }
    );
  }

  public void DOScale(GameObject gameObject, Vector3 scale, float duration, Action onCompleted = null) {
    DOScale(gameObject.transform, scale, duration, onCompleted);
  }

  public void DOAnchorPos(RectTransform rectTransform, Vector2 pos, float duration, Action onCompleted = null) {
    count++;
    rectTransform.DOAnchorPos(pos, duration).OnComplete(
      () => {
        count--;
        onCompleted?.Invoke();
      }
    );
  }

  public void DOAnchorPos(GameObject gameObject, Vector2 pos, float duration, Action onCompleted = null) {
    DOAnchorPos(gameObject.GetRectTransform(), pos, duration, onCompleted);
  }
}