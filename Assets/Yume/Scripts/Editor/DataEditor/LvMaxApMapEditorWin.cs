using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Yume.Editor {
  public class LvMaxApMapEditorWin : EditorWindow {
    private const string JSON_PATH = "Yume/DynamicAssets/Json/lvMaxApMapJson.json";
    private Dictionary<int, int> m_lvMaxApMap;
    private Vector2 m_itemScrollPos;
    #region 映射
    private int m_lv;
    private int m_maxAp;
    #endregion

    [MenuItem("Tools/等级-最大体力映射表编辑器")]
    public static void ShowWin() {
      GetWindow<LvMaxApMapEditorWin>("等级-最大体力映射表编辑器").Show();
    }

    private void OnEnable() {
      string text = File.ReadAllText(Path.Combine(Application.dataPath, JSON_PATH));
      m_lvMaxApMap = JsonConvert.DeserializeObject<Dictionary<int, int>>(text);
    }

    private void OnGUI() {
      bool changed = false;
      EditorGUILayout.LabelField("映射模板");
      m_lv = Mathf.Max(0, EditorGUILayout.IntField("等级", m_lv));
      m_maxAp = Mathf.Max(0, EditorGUILayout.IntField("最大体力", m_maxAp));
      if (GUILayout.Button("添加")) {
        if (!m_lvMaxApMap.ContainsKey(m_lv)) {
          m_lvMaxApMap.Add(m_lv, m_maxAp);
          changed = true;
        }
      }
      EditorGUILayout.Separator();

      m_itemScrollPos = EditorGUILayout.BeginScrollView(m_itemScrollPos);
      List<int> lvs = new List<int>(m_lvMaxApMap.Keys);
      foreach (var l in lvs) {
        EditorGUILayout.Separator();
        int lv = l;
        int maxAP = m_lvMaxApMap[l];
        lv = Mathf.Max(0, EditorGUILayout.IntField("等级", lv));
        maxAP = Mathf.Max(0, EditorGUILayout.IntField("最大体力", maxAP));
        if (GUILayout.Button("删除")) {
          m_lvMaxApMap.Remove(l);
          changed = true;
        } else if (m_lvMaxApMap[l] != maxAP || lv != l) {
          if (lv == l) {
            m_lvMaxApMap[lv] = maxAP;
            changed = true;
          } else if (!m_lvMaxApMap.ContainsKey(lv)) {
            m_lvMaxApMap.Remove(l);
            m_lvMaxApMap.Add(lv, maxAP);
            changed = true;
          }
        }
      }
      if (changed) {
        File.WriteAllText(Path.Combine(Application.dataPath, JSON_PATH), JsonConvert.SerializeObject(m_lvMaxApMap));
        AssetDatabase.Refresh();
      }
      EditorGUILayout.EndScrollView();
    }
  }
}