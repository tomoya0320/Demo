using System;
using UnityEngine;
using UnityEngine.UI;

namespace Yume.UI {
  public class UILevelPanelNoBattle : UIGaussianBlurBg {
    private Text m_title;
    private Text m_description;
    private RectTransform m_rewards;
    private LevelTemplate m_levelTemplate;

    public override void OnInit(UITemplate uiTemplate, GameObject gameObject) {
      base.OnInit(uiTemplate, gameObject);
      
      m_title = GetComponent<Text>("title");
      m_description = GetComponent<Text>("description");
      m_rewards = GetComponent<RectTransform>("rewards");

      _AddListener();
    }

    private void _AddListener() {
      GetComponent<Button>("close").onClick.AddListener(() => UIManager.CloseOne(name));

      Action action = () => DataManager.CompleteLevel(m_levelTemplate.id);
      GetComponent<Button>("start").onClick.AddListener(() => {
        UIManager.OpenOne(
          "globalBlackFade", () => {
            string lateBGM = AudioManager.GetCurrentBgmName();
            UIManager.OpenOne("avg", null, m_levelTemplate.beforeAVGId, action, lateBGM);
          }
        );
      });
    }

    private void _UpdateLevelInfo() {
      m_title.text = m_levelTemplate.title;
      m_description.text = StringUtil.GetActualDescription(m_levelTemplate.description, m_levelTemplate.buffTemplates);
      // 首通奖励
      for (int i = 0; i < m_rewards.childCount; i++) {
        var tempGo = m_rewards.GetChild(i).gameObject;
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
    }

    public override void Open(Action callback = null, params object[] args) {
      m_levelTemplate = (LevelTemplate)args[0];
      _UpdateLevelInfo();

      base.Open(callback, args);
    }
  }
}