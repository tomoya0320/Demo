using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Yume.Data;
using Yume.UI;

namespace Yume.Editor {
  public class UIEditorWin : EditorWindow {
    private const string JSON_PATH = "Yume/DynamicAssets/Json/uiJson.json";
    private const string PREFAB_FOLDER = "../Demo/Assets/Yume/DynamicAssets/UI/Prefab";
    private const string PREFAB_ASSET_PATH = "Assets/Yume/DynamicAssets/UI/Prefab/{0}.prefab";
    private Dictionary<string, UITemplate> m_uiDict;
    private Vector2 m_itemScrollPos;
    #region UI属性
    private UIType m_uiType;
    private float m_cacheTime;
    private int m_typeNameIndex;
    private int m_prefabNameIndex;
    private List<string> m_typeNames = new List<string>();
    private List<string> m_prefabNames = new List<string>();
    #endregion

    [MenuItem("Tools/UI编辑器")]
    public static void ShowWin() {
      GetWindow<UIEditorWin>("UI编辑器").Show();
    }

    private void OnEnable() {
      foreach (var type in typeof(UIBase).Assembly.GetTypes()) {
        if (typeof(UIBase).IsAssignableFrom(type) && !type.IsAbstract) {
          m_typeNames.Add(type.FullName);
        }
      }
      foreach (var path in Directory.GetFiles(PREFAB_FOLDER)) {
        if (Path.GetExtension(path) == ".prefab") {
          m_prefabNames.Add(Path.GetFileNameWithoutExtension(path));
        }
      }

      string text = File.ReadAllText(Path.Combine(Application.dataPath, JSON_PATH));
      m_uiDict = JsonConvert.DeserializeObject<Dictionary<string, UITemplate>>(text);
    }

    private void OnGUI() {
      bool changed = false;
      EditorGUILayout.LabelField("UI模板");
      EditorGUILayout.LabelField("UI名称", m_prefabNames[m_prefabNameIndex]);
      m_uiType = (UIType)EditorGUILayout.EnumPopup("UI类型", m_uiType);
      m_typeNameIndex = EditorGUILayout.Popup("UI类名", m_typeNameIndex, m_typeNames.ToArray());
      m_prefabNameIndex = EditorGUILayout.Popup("UI预制体", m_prefabNameIndex, m_prefabNames.ToArray());
      if (GUILayout.Button("选中预制体")) {
        var obj = AssetDatabase.LoadAssetAtPath<GameObject>(string.Format(PREFAB_ASSET_PATH, m_prefabNames[m_prefabNameIndex]));
        EditorGUIUtility.PingObject(obj);
      }
      m_cacheTime = EditorGUILayout.FloatField("UI缓存时间", m_cacheTime);
      if (GUILayout.Button("添加")) {
        if (!string.IsNullOrEmpty(m_prefabNames[m_prefabNameIndex]) &&
            !m_uiDict.ContainsKey(m_prefabNames[m_prefabNameIndex])) {

          m_uiDict.Add(m_prefabNames[m_prefabNameIndex], new UITemplate {
            name = m_prefabNames[m_prefabNameIndex],
            uiType = m_uiType,
            typeName = m_typeNames[m_typeNameIndex],
            prefabName = m_prefabNames[m_prefabNameIndex],
            cacheTime = m_cacheTime
          });
          changed = true;
        }
      }
      EditorGUILayout.Separator();

      m_itemScrollPos = EditorGUILayout.BeginScrollView(m_itemScrollPos);
      List<string> uiNames = new List<string>(m_uiDict.Keys);
      foreach (var uiName in uiNames) {
        EditorGUILayout.Separator();
        UITemplate uiTemplate = m_uiDict[uiName];
        EditorGUILayout.LabelField("UI名称", uiTemplate.name);
        uiTemplate.uiType = (UIType)EditorGUILayout.EnumPopup("UI类型", uiTemplate.uiType);
        uiTemplate.typeName = m_typeNames[EditorGUILayout.Popup("UI类名", m_typeNames.IndexOf(uiTemplate.typeName), m_typeNames.ToArray())];
        uiTemplate.prefabName = m_prefabNames[EditorGUILayout.Popup("UI预制体", m_prefabNames.IndexOf(uiTemplate.prefabName), m_prefabNames.ToArray())];
        if (GUILayout.Button("选中预制体")) {
          var obj = AssetDatabase.LoadAssetAtPath<GameObject>(string.Format(PREFAB_ASSET_PATH, uiTemplate.prefabName));
          EditorGUIUtility.PingObject(obj);
        }
        uiTemplate.cacheTime = EditorGUILayout.FloatField("UI缓存时间", uiTemplate.cacheTime);
        uiTemplate.name = uiTemplate.prefabName;
        if (GUILayout.Button("删除")) {
          m_uiDict.Remove(uiName);
          changed = true;
        } else if (!m_uiDict[uiName].Equals(uiTemplate)) {
          if (uiTemplate.name == uiName) {
            m_uiDict[uiTemplate.name] = uiTemplate;
            changed = true;
          } else if (!m_uiDict.ContainsKey(uiTemplate.name)) {
            m_uiDict.Remove(uiName);
            m_uiDict.Add(uiTemplate.name, uiTemplate);
            changed = true;
          }
        }
      }
      if (changed) {
        File.WriteAllText(Path.Combine(Application.dataPath, JSON_PATH), JsonConvert.SerializeObject(m_uiDict));
        AssetDatabase.Refresh();
      }
      EditorGUILayout.EndScrollView();
    }
  }
}