using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Yume.Battle;
using Yume.Resource;

public static class Constant {
  public const int MAX_TEAN_COUNT = 5;
  public const int MAX_CARD_COUNT = 12;
  public static readonly Vector2 SCREEN_SIZE = new Vector2(Screen.width, Screen.height);
}

public static class Colors {
  public static readonly Color DISABLE_BUTTON = Color.white * 0.5f;
}

public static class TypeTools {
  public static List<string> GetFieldNames(this Type type) {
    FieldInfo[] fieldInfos = type.GetFields();
    List<string> fieldNameList = new List<string>(fieldInfos.Length);
    foreach (var fieldInfo in fieldInfos) {
      fieldNameList.Add(fieldInfo.Name);
    }
    return fieldNameList;
  }
}

public static class TransformTools {
  /// <summary>
  /// 深度优先遍历查找所有的子物体
  /// </summary>
  /// <param name="transform"></param>
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
}

public static class RectTransformTools {
  public static void SetSizeDeltaX(this RectTransform rt, float x) {
    rt.sizeDelta = new Vector2(x, rt.sizeDelta.y);
  }

  public static void SetSizeDeltaY(this RectTransform rt, float y) {
    rt.sizeDelta = new Vector2(rt.sizeDelta.x, y);
  }
}

public static class ListTools {
  public static T First<T>(this List<T> list) {
    if (list.Count == 0) {
      return default(T);
    }
    return list[0];
  }

  public static T Last<T>(this List<T> list) {
    if (list.Count == 0) {
      return default(T);
    }
    return list[list.Count - 1];
  }
}

public static class TextTools {
  public static void SetAlpha(this Text text, float alpha) {
    Color color = text.color;
    color.a = alpha;
    text.color = color;
  }
}

public static class GameObjectTools {
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
}

public static class EnumTools {
  /// <summary>
  /// 获取当前枚举描述
  /// </summary>
  /// <param name="enumValue"></param>
  /// <returns></returns>
  public static string GetDescription(this Enum enumValue) {
    Type type = enumValue.GetType();
    MemberInfo[] memInfo = type.GetMember(enumValue.ToString());
    if (null != memInfo && memInfo.Length > 0) {
      object[] attrs = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
      if (null != attrs && attrs.Length > 0)
        return ((DescriptionAttribute)attrs[0]).Description;
    }
    return enumValue.ToString();
  }
}

public class AssemblyTool {
  private static Assembly assembly;

  public static object CreateInstance(string typeName) {
    if (assembly == null) {
      assembly = Assembly.GetExecutingAssembly();
    }
    return assembly.CreateInstance(typeName);
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
  public const string NONE = "无";
  public const string SWITCH = "Switch";
  public const string VALUE = "Value";
  public const string ID = "id";
  public const string PRELOAD = "Preload";
  public const string ASSETS_DOWNLOADING = "下载资源中...{0}%(总大小:{1}MB)";
  public const string TASK_COMPLETED = "任务[{0}]已完成！";
  public const string QUIT_GAME_TIP = "确认退出游戏？";
  public const string QUIT = "退出";
  public const string CANCEL = "取消";
  public const string DOWNLOAD_ERROR_TIP = "下载出错，是否重试？";
  public const string RETRY = "重试";
  public const string RETURN_TO_LOGIN = "返回登录界面";
  public const string COMPLETED = "已完成";
  public const string CLICK_GET = "点击领取";
  public const string GOT = "已领取";
  public const string GET = "获得";
  public const string LOGIN = "login";
  public const string HOME = "home";
  public const string LOGIN_BGM = "SOTE_Shrine_v2";
  public const string HOME_BGM = "STS_Merchant_v2";
  public const string DEFAULT_CLICK_SOUND = "点击2";
  public const string DIAMOND_ID = "item_1_diamond";
  public const string GOLD_ID = "item_2_gold";
  public const string GET_EXP = "获得经验值{0}";
  public const string LEVEL_UP_TO = "等级提升至Lv{0}";
  public const string AVG = "avg";
  public const string BG = "bg";
  public const string NAME = "name";
  public const string DIALOGUE = "dialogue";
  public const string UI_OPEN_ANIM = "ui_open";
  public const string UI_CLOSE_ANIM = "ui_close";
  public const string UI_RED_POINT = "red point";
  public const string SAVE_DATA_SUCCEED = "同步数据成功！";
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
  /// 将描述里类似{atk}的字符串替换成blackboard里的实际数值
  /// </summary>
  public static string GetActualDescription(string description, Blackboard blackboard) {
    StringBuilder result = new StringBuilder();
    int start = 0;
    for (int i = 0; i < description.Length; i++) {
      if (description[i] == '{') {
        result.Append(description.Substring(start, i - start));
        start = i + 1;
      } else if (description[i] == '}') {
        result.Append(blackboard.GetValueOrDefault(description.Substring(start, i - start), 0));
        start = i + 1;
      }
    }
    if (start < description.Length) {
      result.Append(description.Substring(start));
    }
    return result.ToString();
  }
}

public static class MathUtil {
  public static Vector2 mousePos => Input.mousePosition;
  public static Vector2 mousePosNormalized => mousePos / Constant.SCREEN_SIZE;

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

  public static Attrib[] CalculateAttribs(string id, int level, int maxLevel, Attrib[] minAttribs, Attrib[] maxAttribs) {

    if (minAttribs.Length != maxAttribs.Length) {
      throw new Exception("最小属性数组和最大属性数组长度不一致！");
    }

    float p = (float)level / maxLevel;
    Attrib[] attribs = new Attrib[minAttribs.Length];
    var curve = ResourceManager.Load<AnimationCurve>(id);
    for (int i = 0; i < attribs.Length; i++) {
      attribs[i] = minAttribs[i] + (maxAttribs[i] - minAttribs[i]) * curve.Evaluate(p);
    }

    return attribs;
  }
}

public abstract class SingletonMono<T> : MonoBehaviour where T : SingletonMono<T> {
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
  public static bool hasInstance {
    get {
      return s_instance != null;
    }
  }

  private void Awake() {
    if (!hasInstance) {
      s_instance = this as T;
      OnInit();
    } else {
      Debug.LogWarning("Duplicate Instacne:" + nameof(T));
      Destroy(gameObject);
    }
  }

  protected virtual void OnInit() { }
}