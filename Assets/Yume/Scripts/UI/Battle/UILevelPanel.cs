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
    private RectTransform m_enemies;
    private RectTransform m_regularRewards;
    private RectTransform m_firstCompletedRewards;

    public override void OnInit(UITemplate uiTemplate, GameObject gameObject) {
      base.OnInit(uiTemplate, gameObject);

      m_icon = GetComponent<Image>("icon");
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
        if (string.IsNullOrEmpty(DataManager.userTeams[DataManager.userTeamIndex].charId)) {
          MessageManager.AddMessage(new Message(Message.Style.FADE, "未携带角色！"));
          return;
        }

        LevelTemplate levelTemplate = ResourceManager.Load<LevelTemplate>(m_level.id);

        Action action = () => {
          if (levelTemplate.enemyTemplates.Length > 0) {
            UIManager.OpenOne("single_battle", null, levelTemplate);
          } else {
            // TODO:直接打开结算界面 测试用
            DataManager.CompleteLevel(levelTemplate.id);
          }
        };

        if (string.IsNullOrEmpty(levelTemplate.beforeAVGId)) {
          UIManager.OpenOne("globalBlackFade", action);
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
          ItemData itemData = levelTemplate.regularRewards.items[i];
          tempGo.SetActiveEx(true);
          tempGo.GetComponentInChildren<Image>().sprite = ResourceManager.Load<Sprite>
            (ResourceManager.Load<ItemTemplate>(itemData.id).iconFileName);
          tempGo.GetComponentInChildren<Text>().text = itemData.count.ToString();
        } else {
          tempGo.SetActiveEx(false);
        }
      }
      // 首通奖励
      for (int i = 0; i < m_firstCompletedRewards.childCount; i++) {
        var tempGo = m_firstCompletedRewards.GetChild(i).gameObject;
        if (i < levelTemplate.firstCompletedRewards.items.Length) {
          ItemData itemData = levelTemplate.firstCompletedRewards.items[i];
          tempGo.SetActiveEx(true);
          tempGo.GetComponentInChildren<Image>().sprite = ResourceManager.Load<Sprite>
            (ResourceManager.Load<ItemTemplate>(itemData.id).iconFileName);
          tempGo.GetComponentInChildren<Text>().text = itemData.count.ToString();
        } else {
          tempGo.SetActiveEx(false);
        }
      }
      // 敌方情报
      for (int i = 0; i < m_enemies.childCount; i++) {
        var tempGo = m_enemies.GetChild(i).gameObject;
        if (i < levelTemplate.enemyTemplates.Length) {
          // 小 心 闭 包
          UnitData enemyData = levelTemplate.enemyTemplates[i].enemy;
          tempGo.SetActiveEx(true);
          tempGo.GetComponentInChildren<Image>().sprite = ResourceManager.Load<Sprite>
            (ResourceManager.Load<UnitTemplate>(enemyData.id).headFileName);
          tempGo.GetComponentInChildren<Button>().onClick.AddListener(() => UIManager.OpenOne("unit_detail", null, enemyData));
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