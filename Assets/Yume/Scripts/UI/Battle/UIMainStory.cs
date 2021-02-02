using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Yume.Data;
using Yume.Resource;

namespace Yume.UI {
  public class UIMainStory : UIBase {
    private Button[] m_levelBtns;
    private List<LevelData> m_levels;

    public override void OnInit(UITemplate uiTemplate, GameObject root) {
      base.OnInit(uiTemplate, root);

      m_levelBtns = GetGameObject("levels").GetComponentsInChildren<Button>(true);

      m_levels = DataManager.userLevels;

      _AddListener();
    }

    public override void Open(Action callback = null, params object[] args) {
      for (int i = 0; i < m_levelBtns.Length && i < m_levels.Count; i++) {
        m_levelBtns[i].gameObject.SetActiveEx(_CheckLevelActive(m_levels[i].id));
      }

      base.Open(callback, args);
    }

    private void _AddListener() {
      GetComponent<Button>("close").onClick.AddListener(() => UIManager.CloseOne(name));
      for (int i = 0; i < m_levelBtns.Length; i++) {
        if (i < m_levels.Count) {
          var level = m_levels[i];
          m_levelBtns[i].gameObject.SetActiveEx(_CheckLevelActive(m_levels[i].id));
          m_levelBtns[i].onClick.AddListener(() => {
            UIManager.OpenOne("level_panel", null, level);
          });
        } else {
          m_levelBtns[i].gameObject.SetActiveEx(false);
        }
      }
    }

    private bool _CheckLevelActive(string id) {
      string preLevelId = ResourceManager.Load<LevelTemplate>(id).preLevelId;
      return string.IsNullOrEmpty(preLevelId) || m_levels.Find(l => l.id == preLevelId).completed;
    }
  }
}