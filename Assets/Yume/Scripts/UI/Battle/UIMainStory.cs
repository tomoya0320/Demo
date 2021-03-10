using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Yume.Broadcast;
using Yume.Data;
using Yume.Resource;
using EventType = Yume.Broadcast.EventType;

namespace Yume.UI {
  public class UIMainStory : UIBase {
    private Button[] m_levelBtns;
    private List<LevelData> m_levels;

    public override void OnInit(UITemplate uiTemplate, GameObject gameObject) {
      base.OnInit(uiTemplate, gameObject);

      m_levelBtns = GetGameObject("levels").GetComponentsInChildren<Button>(true);

      m_levels = DataManager.playerLevels.FindAll(l => ResourceManager.Load<LevelTemplate>(l.id).type == LevelType.MAIN_STORY);

      _AddListener();

      EventCenter.AddListener(EventType.UPDATE_MAIN_STORY_LEVEL, _UpdateLevels);
    }

    public override void Destroy() {
      base.Destroy();
      
      EventCenter.RemoveListener(EventType.UPDATE_MAIN_STORY_LEVEL, _UpdateLevels);
    }

    public override void Open(Action callback = null, params object[] args) {
      _UpdateLevels();

      base.Open(callback, args);
    }

    private void _UpdateLevels() {
      for (int i = 0; i < m_levelBtns.Length && i < m_levels.Count; i++) {
        bool active = _CheckLevelActive(m_levels[i].id);
        m_levelBtns[i].gameObject.SetActiveEx(active);
        var objectContainer = m_levelBtns[i].gameObject.GetComponent<ObjectContainer>();
        objectContainer?.GetGameObject("line")?.SetActiveEx(active);
        m_levelBtns[i].gameObject.FindGo("completed")?.SetActiveEx(m_levels[i].completed);
      }
    }

    private void _AddListener() {
      GetComponent<Button>("close").onClick.AddListener(() => UIManager.CloseOne(name));
      for (int i = 0; i < m_levelBtns.Length; i++) {
        if (i < m_levels.Count) {
          LevelTemplate levelTemplate = ResourceManager.Load<LevelTemplate>(m_levels[i].id);
          m_levelBtns[i].gameObject.SetActiveEx(_CheckLevelActive(m_levels[i].id));
          m_levelBtns[i].onClick.AddListener(() => {
            if (levelTemplate.enemyTeams.Length > 0) {
              UIManager.OpenOne("level_panel", null, levelTemplate);
            } else {
              UIManager.OpenOne("level_panel_no_battle", null, levelTemplate);
            }
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