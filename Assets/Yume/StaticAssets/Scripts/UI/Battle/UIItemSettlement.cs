using System;
using UnityEngine;
using UnityEngine.UI;

namespace Yume.UI {
  public class UIItemSettlement : UIBase {
    private RectTransform m_rewards;

    public override void OnInit(UITemplate uiTemplate, GameObject gameObject) {
      base.OnInit(uiTemplate, gameObject);
      
      m_rewards = GetComponent<RectTransform>("rewards");

      _AddListener();
    }

    private void _AddListener() {
      GetComponent<Button>("close").onClick.AddListener(() => UIManager.CloseOne(name));
    }

    public override void Open(Action callback = null, params object[] args) {
      ItemArgs[] itemArgsArray = args[0] as ItemArgs[];
      // 显示奖励
      for (int i = 0; i < m_rewards.childCount; i++) {
        var tempGo = m_rewards.GetChild(i).gameObject;
        if (i < itemArgsArray.Length) {
          ItemArgs itemArgs = itemArgsArray[i];
          tempGo.SetActiveEx(true);
          tempGo.GetComponentInChildren<Image>().SetSprite(ResourceManager.Load<ItemTemplate>(itemArgs.id).iconFileName);
          tempGo.GetComponentInChildren<Text>().text = itemArgs.count.ToString();
        } else {
          tempGo.SetActiveEx(false);
        }
      }
      
      base.Open(callback, args);
    }
  }
}