using System;
using System.ComponentModel;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

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