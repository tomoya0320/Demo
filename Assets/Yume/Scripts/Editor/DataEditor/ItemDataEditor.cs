using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Yume.Data;

namespace Yume.Editor {
  public class ItemDataEditor : EditorWindow {
    private const string JSON_PATH = "Yume/DynamicAssets/Json/itemJson.json";
    private Dictionary<string, ItemTemplate> m_itemDict;
    private Vector2 m_itemScrollPos;
    #region 道具属性
    private string m_id;
    private string m_name;
    private string m_description;
    private string m_iconFileName;
    #endregion

    [MenuItem("Tools/道具编辑器")]
    public static void ShowWin() {
      GetWindow<ItemDataEditor>("道具编辑器").Show();
    }

    private void Awake() {
      string text = File.ReadAllText(Path.Combine(Application.dataPath, JSON_PATH));
      m_itemDict = JsonConvert.DeserializeObject<Dictionary<string, ItemTemplate>>(text);
    }

    private void OnGUI() {
      bool changed = false;
      EditorGUILayout.LabelField("道具模板");
      m_id = EditorGUILayout.TextField("道具id", m_id);
      m_name = EditorGUILayout.TextField("道具名", m_name);
      m_description = EditorGUILayout.TextField("道具描述", m_description);
      m_iconFileName = EditorGUILayout.TextField("道具图标", m_iconFileName);
      if (GUILayout.Button("添加")) {
        if (!string.IsNullOrEmpty(m_id) && !m_itemDict.ContainsKey(m_id)) {
          m_itemDict.Add(m_id, new ItemTemplate {
            id = m_id,
            name = m_name,
            description = m_description,
            iconFileName = m_iconFileName
          });
          changed = true;
        }
      }
      EditorGUILayout.Separator();

      m_itemScrollPos = EditorGUILayout.BeginScrollView(m_itemScrollPos);
      List<string> itemIds = new List<string>(m_itemDict.Keys);
      foreach (var itemId in itemIds) {
        EditorGUILayout.Separator();
        ItemTemplate itemTemplate = m_itemDict[itemId];
        itemTemplate.id = EditorGUILayout.TextField("道具id", itemTemplate.id);
        itemTemplate.name = EditorGUILayout.TextField("道具名", itemTemplate.name);
        itemTemplate.description = EditorGUILayout.TextField("道具描述", itemTemplate.description);
        itemTemplate.iconFileName = EditorGUILayout.TextField("道具图标", itemTemplate.iconFileName);
        if (GUILayout.Button("删除")) {
          m_itemDict.Remove(itemId);
          changed = true;
        } else if (!m_itemDict[itemId].Equals(itemTemplate)) {
          m_itemDict[itemId] = itemTemplate;
          changed = true;
        }
      }
      if (changed) {
        File.WriteAllText(Path.Combine(Application.dataPath, JSON_PATH), JsonConvert.SerializeObject(m_itemDict));
        AssetDatabase.Refresh();
      }
      EditorGUILayout.EndScrollView();
    }
  }
}