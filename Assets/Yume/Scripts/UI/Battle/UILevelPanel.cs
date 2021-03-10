using System;
using UnityEngine;
using UnityEngine.UI;
using Yume.Data;
using Yume.Resource;

namespace Yume.UI {
  public class UILevelPanel : UIBase {
    private Text m_title;
    private Text m_description;
    private RectTransform m_enemies;
    private LevelTemplate m_levelTemplate;
    private RectTransform m_regularRewards;
    private RectTransform m_firstCompletedRewards;

    public override void OnInit(UITemplate uiTemplate, GameObject gameObject) {
      base.OnInit(uiTemplate, gameObject);
      
      m_title = GetComponent<Text>("title");
      m_description = GetComponent<Text>("description");
      m_enemies = GetComponent<RectTransform>("enemies");
      m_regularRewards = GetComponent<RectTransform>("regularRewards");
      m_firstCompletedRewards = GetComponent<RectTransform>("firstCompletedRewards");

      _AddListener();
    }

    private void _AddListener() {
      GetComponent<Button>("close").onClick.AddListener(() => UIManager.CloseOne(name));

      GetComponent<Button>("startBattleBtn").onClick.AddListener(() => {
        UIManager.OpenOne("team_before_battle", null, m_levelTemplate);
      });
    }

    private void _UpdateLevelInfo() {
      m_title.text = m_levelTemplate.title;
      m_description.text = StringUtil.GetActualDescription(m_levelTemplate.description, m_levelTemplate.buffTemplates);
      // 常规奖励
      for (int i = 0; i < m_regularRewards.childCount; i++) {
        var tempGo = m_regularRewards.GetChild(i).gameObject;
        if (i < m_levelTemplate.regularRewards.items.Length) {
          ItemArgs itemArgs = m_levelTemplate.regularRewards.items[i];
          tempGo.SetActiveEx(true);
          tempGo.GetComponentInChildren<Image>().sprite = ResourceManager.Load<Sprite>
            (ResourceManager.Load<ItemTemplate>(itemArgs.id).iconFileName);
          tempGo.GetComponentInChildren<Text>().text = itemArgs.count.ToString();
        } else {
          tempGo.SetActiveEx(false);
        }
      }
      // 首通奖励
      for (int i = 0; i < m_firstCompletedRewards.childCount; i++) {
        var tempGo = m_firstCompletedRewards.GetChild(i).gameObject;
        if (i < m_levelTemplate.firstCompletedRewards.items.Length) {
          ItemArgs itemArgs = m_levelTemplate.firstCompletedRewards.items[i];
          tempGo.SetActiveEx(true);
          tempGo.GetComponentInChildren<Image>().sprite = ResourceManager.Load<Sprite>
            (ResourceManager.Load<ItemTemplate>(itemArgs.id).iconFileName);
          tempGo.GetComponentInChildren<Text>().text = itemArgs.count.ToString();
        } else {
          tempGo.SetActiveEx(false);
        }
      }
      // 敌方情报
      for (int i = 0; i < m_enemies.childCount; i++) {
        var tempGo = m_enemies.GetChild(i).gameObject;
        if (i < m_levelTemplate.enemyTeams.Length) {
          // 小 心 闭 包
          UnitArgs enemyArgs = m_levelTemplate.enemyTeams[i].unitArgs;
          tempGo.SetActiveEx(true);
          tempGo.GetComponentInChildren<Image>().sprite = ResourceManager.Load<Sprite>
            (ResourceManager.Load<UnitTemplate>(enemyArgs.id).headFileName);
          tempGo.GetComponentInChildren<Button>().onClick.AddListener(() => UIManager.OpenOne("enemy_detail", null, enemyArgs));
        } else {
          tempGo.SetActiveEx(false);
        }
      }
    }

    public override void Open(Action callback = null, params object[] args) {
      m_levelTemplate = (LevelTemplate)args[0];
      _UpdateLevelInfo();

      base.Open(callback, args);
    }
  }
}