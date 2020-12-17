using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// List拓展工具类
/// </summary>
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

/// <summary>
/// Image拓展工具类
/// </summary>
public static class ImageTools {
  public static void SetAlpha(this Image image, float alpha) {
    Color color = image.color;
    color.a = alpha;
    image.color = color;
  }
}

/// <summary>
/// GameObject拓展工具类
/// </summary>
public static class GameObjectTools {
  public static RectTransform GetRect(this GameObject gameObject) {
    return gameObject.GetComponent<RectTransform>();
  }
}

/// <summary>
/// 枚举拓展工具类
/// </summary>
public static class EnumTools {
  /// <summary>
  /// 获取当前枚举描述
  /// </summary>
  /// <param name="enumValue"></param>
  /// <returns></returns>
  public static string GetEnumDescription(this Enum enumValue) {
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
  protected bool dontDestroyOnLoad = true;

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
    if (s_instance == null) {
      s_instance = this as T;
      OnInit();
    } else {
      Debug.LogWarning("Duplicate Instacne:" + typeof(T).Name);
      Destroy(gameObject);
    }
  }

  protected virtual void OnInit() {
    if (dontDestroyOnLoad) {
      DontDestroyOnLoad(gameObject);
    }
  }
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