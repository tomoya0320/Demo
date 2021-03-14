using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Yume.Broadcast;
using EventType = Yume.Broadcast.EventType;

namespace Yume.UI {
  public class UIMainStory : UIBase {
    private class UILevel {
      public Button button;
      public GameObject line;
      public GameObject completed;
      public GameObject gameObject;

      public UILevel(Button button, GameObject line, GameObject completed, GameObject gameObject) {
        this.line = line;
        this.button = button;
        this.completed = completed;
        this.gameObject = gameObject;
      }
    }

    private List<LevelData> m_levels;
    private Dictionary<string, UILevel> m_uiLevelDict = new Dictionary<string, UILevel>();

    public override void OnInit(UITemplate uiTemplate, GameObject gameObject) {
      base.OnInit(uiTemplate, gameObject);

      Transform levels = GetTransform("levels");
      for (int i = 0; i < levels.childCount; i++) {
        GameObject tempGo = levels.GetChild(i).gameObject;
        ObjectContainer objectContainer = tempGo.GetComponent<ObjectContainer>();
        Button button = objectContainer.GetComponent<Button>("button");
        GameObject line = objectContainer.GetGameObject("line");
        GameObject completed = objectContainer.GetGameObject("completed");
        m_uiLevelDict.Add(tempGo.name, new UILevel(button, line, completed, tempGo));
      }

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
      foreach (var level in m_levels) {
        if (m_uiLevelDict.ContainsKey(level.id)) {
          bool active = _CheckLevelActive(level.id);
          UILevel uiLevel = m_uiLevelDict[level.id];
          uiLevel.gameObject.SetActiveEx(active);
          uiLevel.line?.SetActiveEx(active);
          uiLevel.completed?.SetActiveEx(level.completed);
        }
      }
    }

    private void _AddListener() {
      GetComponent<Button>("close").onClick.AddListener(() => UIManager.CloseOne(name));
      foreach (var level in m_levels) {
        if (m_uiLevelDict.ContainsKey(level.id)) {
          m_uiLevelDict[level.id].button.onClick.AddListener(
            () => {
              var levelTemplate = ResourceManager.Load<LevelTemplate>(level.id);
              if (levelTemplate.enemyTeams.Length > 0) {
                UIManager.OpenOne("level_panel", null, levelTemplate);
              } else {
                UIManager.OpenOne("level_panel_no_battle", null, levelTemplate);
              }
            }
          );
        }
      }
    }

    private bool _CheckLevelActive(string id) {
      string preLevelId = ResourceManager.Load<LevelTemplate>(id).preLevelId;
      return string.IsNullOrEmpty(preLevelId) || m_levels.Find(l => l.id == preLevelId).completed;
    }
  }
}