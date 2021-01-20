using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

public static class Constant {
  public const int MAX_TEAN_NUM = 5;
  public const int MAX_CARD_NUM = 12;
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

public abstract class Singleton<T> where T : new() {
  private static T s_instance;

  public static T instance {
    get {
      if (s_instance == null) {
        s_instance = new T();
      }
      return s_instance;
    }
  }
  public static bool hasInstance {
    get {
      return s_instance != null;
    }
  }

  protected Singleton() {
    OnInit();
  }

  protected virtual void OnInit() { }
}