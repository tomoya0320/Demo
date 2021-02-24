using System;
using UnityEngine;
using UnityEngine.UI;
using Yume.Battle;
using Yume.Broadcast;
using Yume.Data;
using Yume.Resource;
using EventType = Yume.Broadcast.EventType;

namespace Yume.UI {
  public class UISingleBattle : UIBase {
    private Image m_bg;
    private YButton m_finishTurnBtn;

    public override void OnInit(UITemplate uiTemplate, GameObject gameObject) {
      base.OnInit(uiTemplate, gameObject);

      m_bg = GetComponent<Image>("bg");
      m_finishTurnBtn = GetComponent<YButton>("finish turn");
      
      _AddListener();
      
      EventCenter.AddListener(EventType.ON_ENTER_PLAYER_TURN, _EnterPlayerTurn);
      EventCenter.AddListener(EventType.ON_ENTER_FIRST_ENEMY_TURN, _EnterFirstEnemyTurn);
    }

    private void _AddListener() {
      m_finishTurnBtn.onClick.AddListener(_FinishPlayerTurn);
      GetComponent<Button>("abort").onClick.AddListener(_Abort);
    }

    private void _Abort() {
      if (!BattleManager.isRunning) {
        return;
      }

      if (UIManager.GetTopUI(uiType).name == name) {
        UIManager.OpenOne("confirmation", null, 
          "确认放弃作战？", "确认", "取消", BattleManager.abort, null);
      } else {
        UIManager.CloseOne("confirmation");
      }
    }

    private void _FinishPlayerTurn() {
      m_finishTurnBtn.interactable = false;
      m_finishTurnBtn.image.color = Color.gray;

      BattleManager.character.EnqueueCard(default);
    }

    private void _EnterFirstEnemyTurn() {
      m_finishTurnBtn.textComponent.text = "敌方回合";
      
      MessageManager.AddMessage(new Message(Message.Style.CENTER, "敌方回合", null, 0.5f));
    }

    private void _EnterPlayerTurn() {
      m_finishTurnBtn.interactable = true;
      m_finishTurnBtn.image.color = Color.white;
      m_finishTurnBtn.textComponent.text = "结束回合";
      
      MessageManager.AddMessage(new Message(Message.Style.CENTER, "玩家回合", null, 0.5f));
    }

    public override void Open(Action callback = null, params object[] args) {
      UIManager.onBackList.Add(_Abort);

      LevelTemplate levelTemplate = (LevelTemplate)args[0];
      m_bg.sprite = ResourceManager.Load<Sprite>(levelTemplate.background);

      // 注意初始化顺序
      UnitFactory.Init();
      CardFactory.Init();
      BattleManager.Init(levelTemplate);

      callback += BattleManager.StartBattle;
      base.Open(callback, args);
    }

    public override void Close(Action callback = null, params object[] args) {
      callback += () => UIManager.onBackList.Remove(_Abort);
      base.Close(callback, args);
    }

    public override void Destroy() {
      base.Destroy();
      EventCenter.RemoveListener(EventType.ON_ENTER_PLAYER_TURN, _EnterPlayerTurn);
      EventCenter.RemoveListener(EventType.ON_ENTER_FIRST_ENEMY_TURN, _EnterFirstEnemyTurn);
    }
  }
}