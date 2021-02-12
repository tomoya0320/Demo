using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Yume.Battle;
using Yume.Data;
using Yume.Resource;

namespace Yume.UI {
  public class UISingleBattle : UIBase {
    private Image m_bg;
    private Dictionary<string, Transform> m_folderDict = new Dictionary<string, Transform>();

    public override void OnInit(UITemplate uiTemplate, GameObject root) {
      base.OnInit(uiTemplate, root);

      m_bg = GetComponent<Image>("bg");
      m_folderDict.Add("charFolder", GetGameObject("charFolder").transform);
      m_folderDict.Add("enemyFolder", GetGameObject("enemyFolder").transform);
      m_folderDict.Add("drawCardFolder", GetGameObject("drawCardFolder").transform);
      m_folderDict.Add("discardCardFolder", GetGameObject("discardCardFolder").transform);
      m_folderDict.Add("consumeCardFolder", GetGameObject("consumeCardFolder").transform);

      _AddListener();
    }

    private void _AddListener() {
      GetComponent<Button>("abort").onClick.AddListener(_Abort);
    }

    private void _Abort() {
      if (UIManager.GetTopUI(uiType).name == name) {
        UIManager.OpenOne("confirmation", null, "确认放弃作战？", "确认", "取消", BattleManager.abort, null);
      } else {
        UIManager.CloseOne("confirmation");
      }
    }

    public override void Open(Action callback = null, params object[] args) {
      UIManager.onBackList.Add(_Abort);

      LevelTemplate levelTemplate = (LevelTemplate)args[0];
      m_bg.sprite = ResourceManager.Load<Sprite>(levelTemplate.background);

      BattleManager.Init(levelTemplate, m_folderDict);

      callback += BattleManager.StartBattle;
      base.Open(callback, args);
    }

    public override void Close(Action callback = null, params object[] args) {
      callback += () => UIManager.onBackList.Remove(_Abort);
      base.Close(callback, args);
    }
  }
}