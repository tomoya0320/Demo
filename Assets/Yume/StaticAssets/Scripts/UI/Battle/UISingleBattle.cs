using System;
using UnityEngine;
using UnityEngine.UI;
using Yume.Battle;
using Yume.Broadcast;
using EventType = Yume.Broadcast.EventType;

namespace Yume.UI {
  public class UISingleBattle : UIBase {
    private Image m_bg;
    private YButton m_finishTurnBtn;
    private Action m_abort = () => BattleManager.Finish(BattleResult.ABORT);

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
      GetComponent<Button>("abort").onClick.AddListener(_OnClickAbort);

      GetComponent<Button>("lookDrawCard").onClick.AddListener(
        () =>
          UIManager.OpenOne(
            "card_heap", null,
            "抽牌堆", BattleManager.character.GetCards(CardHeapType.DRAW)
          )
      );
      GetComponent<Button>("lookDiscardCard").onClick.AddListener(
        () =>
          UIManager.OpenOne(
            "card_heap", null,
            "弃牌堆", BattleManager.character.GetCards(CardHeapType.DISCARD)
          )
      );
      GetComponent<Button>("lookConsumeCard").onClick.AddListener(
        () =>
          UIManager.OpenOne(
            "card_heap", null,
            "耗牌堆", BattleManager.character.GetCards(CardHeapType.CONSUME)
          )
      );
    }

    private void _OnClickAbort() {
      if (!BattleManager.isRunning) {
        return;
      }
      
      if (UIManager.currentUI.name == name) {
        UIManager.OpenOne(
          "confirmation", null,
          "确认放弃作战？", "确认", "取消", m_abort, null
        );
      } else {
        UIManager.CloseOne(UIManager.currentUI.name);
      }
    }

    private void _FinishPlayerTurn() {
      m_finishTurnBtn.interactable = false;
      m_finishTurnBtn.image.color = Color.gray;

      BattleManager.character.EnqueueCard(null);
    }

    private void _EnterFirstEnemyTurn() {
      m_finishTurnBtn.textComponent.text = "敌方回合";

      MessageManager.AddMessage(new Message(Message.Style.CENTER, "敌方回合", false, null, 0.5f));
    }

    private void _EnterPlayerTurn() {
      m_finishTurnBtn.interactable = true;
      m_finishTurnBtn.image.color = Color.white;
      m_finishTurnBtn.textComponent.text = "结束回合";

      MessageManager.AddMessage(new Message(Message.Style.CENTER, "玩家回合", false, null, 0.5f));
    }

    public override void Open(Action callback = null, params object[] args) {
      UIManager.onBackList.Add(_OnClickAbort);

      LevelTemplate levelTemplate = (LevelTemplate)args[0];
      // 设置bgm
      uiTemplate.bgm = levelTemplate.bgm;
      // 设置战斗背景
      m_bg.SetSprite(levelTemplate.background);
      // 开始战斗
      BattleManager.StartBattle((TeamArgs)args[1], levelTemplate, objectContainer); 
      
      // 注意顺序
      base.Open(callback, args);
    }

    public override void Close(bool immediate = false, Action callback = null) {
      callback += () => UIManager.onBackList.Remove(_OnClickAbort);
      base.Close(immediate, callback);
    }

    public override void Destroy() {
      base.Destroy();
      EventCenter.RemoveListener(EventType.ON_ENTER_PLAYER_TURN, _EnterPlayerTurn);
      EventCenter.RemoveListener(EventType.ON_ENTER_FIRST_ENEMY_TURN, _EnterFirstEnemyTurn);
    }
  }
}