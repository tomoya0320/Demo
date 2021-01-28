using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Yume.Data;

namespace Yume.Editor {
  public class ItemEditorWin : EditorWindow {
    private const string JSON_PATH = "Yume/DynamicAssets/Json/itemJson.json";
    private const string SPRITE_FOLDER = "../Demo/Assets/Yume/DynamicAssets/UI/Sprite/Item";
    private const string SPRITE_ASSET_PATH = "Assets/Yume/DynamicAssets/UI/Sprite/Item/{0}.png";
    private Dictionary<string, ItemTemplate> m_itemDict;
    private Vector2 m_scrollPos;
    #region 道具属性
    private string m_id;
    private string m_name;
    private string m_description;
    private int m_iconFileNameIndex;
    private List<GUIContent> m_icons = new List<GUIContent>();
    #endregion

    [MenuItem("Tools/道具编辑器")]
    public static void ShowWin() {
      GetWindow<ItemEditorWin>("道具编辑器").Show();
    }

    private void OnEnable() {
      m_icons = EditorWinTools.GetImageWithName(SPRITE_FOLDER, SPRITE_ASSET_PATH);

      string text = File.ReadAllText(Path.Combine(Application.dataPath, JSON_PATH));
      m_itemDict = JsonConvert.DeserializeObject<Dictionary<string, ItemTemplate>>(text);
    }

    private void OnGUI() {
      bool changed = false;
      EditorGUILayout.LabelField("道具模板");
      m_id = EditorGUILayout.TextField("道具id", m_id);
      m_name = EditorGUILayout.TextField("道具名", m_name);
      m_description = EditorGUILayout.TextField("道具描述", m_description);
      m_iconFileNameIndex = EditorGUILayout.Popup(new GUIContent("道具图标"), m_iconFileNameIndex, m_icons.ToArray());
      if (GUILayout.Button("添加")) {
        if (!string.IsNullOrEmpty(m_id) && !m_itemDict.ContainsKey(m_id)) {
          m_itemDict.Add(m_id, new ItemTemplate {
            id = m_id,
            name = m_name,
            description = m_description,
            iconFileName = m_icons[m_iconFileNameIndex].text
          });
          changed = true;
        }
      }
      EditorGUILayout.Separator();

      m_scrollPos = EditorGUILayout.BeginScrollView(m_scrollPos);
      List<string> itemIds = new List<string>(m_itemDict.Keys);
      foreach (var itemId in itemIds) {
        EditorGUILayout.Separator();
        ItemTemplate itemTemplate = m_itemDict[itemId];
        itemTemplate.id = EditorGUILayout.TextField("道具id", itemTemplate.id);
        itemTemplate.name = EditorGUILayout.TextField("道具名", itemTemplate.name);
        itemTemplate.description = EditorGUILayout.TextField("道具描述", itemTemplate.description);
        itemTemplate.iconFileName = m_icons[EditorGUILayout.Popup(new GUIContent("道具图标"),
          m_icons.FindIndex(i => i.text == itemTemplate.iconFileName), m_icons.ToArray())].text;
        if (GUILayout.Button("删除")) {
          m_itemDict.Remove(itemId);
          changed = true;
        } else if (!m_itemDict[itemId].Equals(itemTemplate)) {
          if (itemTemplate.id == itemId) {
            m_itemDict[itemTemplate.id] = itemTemplate;
            changed = true;
          } else if (!m_itemDict.ContainsKey(itemTemplate.id)) {
            m_itemDict.Remove(itemId);
            m_itemDict.Add(itemTemplate.id, itemTemplate);
            changed = true;
          }
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