using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Yume.Editor {
  public static class EditorWinTools {
    public static object EditorGUILayoutField(string label, string type, object data) {
      if (type.ToLower().Contains("string")) {
        if ((data == null) || !(data is string)) {
          data = "";
        }
        return EditorGUILayout.TextField(label, (string)data);
      }
      if (type.ToLower().Contains("int")) {
        if ((data == null) || !(data is int)) {
          data = 0;
        }
        return EditorGUILayout.IntField(label, (int)data);
      }
      if (type.ToLower().Contains("float")) {
        if ((data == null) || !(data is float)) {
          data = 0f;
        }
        return EditorGUILayout.FloatField(label, (float)data);
      }
      if (type.ToLower().Contains("bool")) {
        if ((data == null) || !(data is bool)) {
          data = false;
        }
        return EditorGUILayout.Toggle(label, (bool)data);
      }
      return default;
    }

    public static List<GUIContent> GetImageWithName(string folder, string assetPath) {
      List<GUIContent> result = new List<GUIContent>();

      foreach (var path in Directory.GetFiles(folder)) {
        if (Path.GetExtension(path) == ".png") {
          string name = Path.GetFileNameWithoutExtension(path);
          Texture icon = AssetDatabase.LoadAssetAtPath<Texture>(string.Format(assetPath, name));
          GUIContent content = new GUIContent(name, icon);
          result.Add(content);
        }
      }

      return result;
    }
  }
}