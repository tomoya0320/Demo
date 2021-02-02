using System;
using UnityEngine;
using UnityEngine.UI;
using Yume.Data;
using Yume.Resource;

namespace Yume.UI {
  public class UILevelPanel : UIBase {
    private Image m_icon;
    private Text m_title;
    private LevelData m_level;
    private Text m_description;
    private RectTransform m_regularRewards;
    private RectTransform m_firstCompletedRewards;

    public override void OnInit(UITemplate uiTemplate, GameObject root) {
      base.OnInit(uiTemplate, root);

      m_icon = GetComponent<Image>("icon");
      m_title = GetComponent<Text>("title");
      m_description = GetComponent<Text>("description");
      m_regularRewards = GetComponent<RectTransform>("regularRewards");
      m_firstCompletedRewards = GetComponent<RectTransform>("firstCompletedRewards");

      _AddListener();
    }

    private void _AddListener() {
      GetComponent<Button>("close").onClick.AddListener(() => UIManager.CloseOne(name));

      GetComponent<Button>("startBattleBtn").onClick.AddListener(() => {
        LevelTemplate levelTemplate = ResourceManager.Load<LevelTemplate>(m_level.id);

        Action action = () => {
          // TODO:进入战斗
          Debug.Log("进入战斗");
        };

        if (string.IsNullOrEmpty(levelTemplate.beforeAVGId)) {
          action();
        } else {
          UIManager.OpenOne("globalBlackFade", () => {
            UIManager.OpenOne("avg", null, levelTemplate.beforeAVGId, action);
          });
        }
      });
    }

    private void _UpdateLevelInfo() {
      LevelTemplate levelTemplate = ResourceManager.Load<LevelTemplate>(m_level.id);
      m_icon.sprite = ResourceManager.Load<Sprite>(levelTemplate.iconFileName);
      m_title.text = levelTemplate.title;
      m_description.text = levelTemplate.description;
      // 常规奖励
      for (int i = 0; i < m_regularRewards.childCount; i++) {
        var tempGo = m_regularRewards.GetChild(i).gameObject;
        if (i < levelTemplate.regularRewards.items.Length) {
          tempGo.SetActiveEx(true);
          tempGo.GetComponentInChildren<Image>().sprite = ResourceManager.Load<Sprite>
                 (ResourceManager.Load<ItemTemplate>(levelTemplate.regularRewards.items[i].id).iconFileName);
          tempGo.GetComponentInChildren<Text>().text = levelTemplate.regularRewards.items[i].count.ToString();
        } else {
          tempGo.SetActiveEx(false);
        }
      }
      // 首通奖励
      for (int i = 0; i < m_firstCompletedRewards.childCount; i++) {
        var tempGo = m_firstCompletedRewards.GetChild(i).gameObject;
        if (i < levelTemplate.firstCompletedRewards.items.Length) {
          tempGo.SetActiveEx(true);
          tempGo.GetComponentInChildren<Image>().sprite = ResourceManager.Load<Sprite>
                 (ResourceManager.Load<ItemTemplate>(levelTemplate.firstCompletedRewards.items[i].id).iconFileName);
          tempGo.GetComponentInChildren<Text>().text = levelTemplate.firstCompletedRewards.items[i].count.ToString();
        } else {
          tempGo.SetActiveEx(false);
        }
      }
    }

    public override void Open(Action callback = null, params object[] args) {
      m_level = args[0] as LevelData;
      _UpdateLevelInfo();

      base.Open(callback, args);
    }
  }
}