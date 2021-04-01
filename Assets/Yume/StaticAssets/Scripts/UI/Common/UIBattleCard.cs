using System;
using UnityEngine;
using UnityEngine.EventSystems;
using Yume.Battle;
using Random = UnityEngine.Random;

namespace Yume.UI {
  public enum BattleCardState {
    OUT_HAND,
    IN_HAND,
    DRAGGING,
    PLAYING,
    ADDING,
    REMOVING,
  }
  
  [RequireComponent(typeof(RectTransform))]
  public class UIBattleCard : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {
    #region StateMachine
    
    private class OutHand : StateBase<UIBattleCard> {
      public OutHand(UIBattleCard owner) : base((int)BattleCardState.OUT_HAND, owner) { }

      public override void OnEnter(int lastStateId) {
        owner.gameObject.SetActiveEx(false);
      }

      public override void OnExit(int nextStateId) {
        owner.gameObject.SetActiveEx(true);
      }
    }
    
    private class Adding : StateBase<UIBattleCard> {
      private float m_speed;
      
      public Adding(UIBattleCard owner) : base((int)BattleCardState.ADDING, owner) { }

      public override void OnEnter(int lastStateId) {
        m_speed = speed;
        owner.transform.localScale = smallScale;
        owner.transform.SetParent(CardFactory.GetFolder(CardHeapType.HAND));
      }

      public override void OnTrick() {
        if (Vector2.Distance(owner.m_targetAnchorPos, owner.anchoredPosition) < MIN_DIST) {
          owner.transform.localScale = Vector3.one;
          owner.anchoredPosition = owner.m_targetAnchorPos;
          owner.transform.SetAngleZ(owner.m_targetAngle);
          stateMachine.SwitchState((int)BattleCardState.IN_HAND);
        } else {
          owner.transform.localScale = Vector3.Lerp(owner.transform.localScale, Vector3.one, m_speed);
          owner.anchoredPosition = Vector2.Lerp(owner.anchoredPosition, owner.m_targetAnchorPos, m_speed);
          owner.transform.SetAngleZ(Mathf.LerpAngle(owner.transform.eulerAngles.z, owner.m_targetAngle, m_speed));
          m_speed += speedIncrement;
        }
      }
    }

    private class InHand : StateBase<UIBattleCard> {
      public InHand(UIBattleCard owner) : base((int)BattleCardState.IN_HAND, owner) { }

      public override void OnEnter(int lastStateId) {
        owner.transform.SetParent(CardFactory.GetFolder(CardHeapType.HAND));
        owner.transform.SetSiblingIndex(owner.m_targetSiblingIndex);
      }

      public override void OnTrick() {
        owner.transform.localScale = Vector3.Lerp(owner.transform.localScale, Vector3.one, SPEED_SCALE * speed);
        owner.anchoredPosition = Vector2.Lerp(owner.anchoredPosition, owner.m_targetAnchorPos, SPEED_SCALE * speed);
        owner.transform.SetAngleZ(Mathf.LerpAngle(owner.transform.eulerAngles.z, owner.m_targetAngle, SPEED_SCALE * speed));
      }
    }

    private class Dragging : StateBase<UIBattleCard> {
      public Dragging(UIBattleCard owner) : base((int)BattleCardState.DRAGGING, owner) { }

      public override void OnEnter(int lastStateId) {
        owner.transform.eulerAngles = Vector3.zero;
        owner.transform.SetParent(CardFactory.handCardTopFolder);
      }

      public override void OnTrick() {
        // 大小缩放
        owner.transform.localScale = Vector3.Lerp(owner.transform.localScale, bigScale, SPEED_SCALE * speed);
        // 位置移动
        owner.anchoredPosition = owner.m_eventData.position - 0.5f * Constant.SCREEN_SIZE;
        // 目标选择
        // 取消之前选择的
        if (owner.m_card.candidate != null) {
          owner.m_card.candidate.SetSelectedEnable(false);
          owner.m_card.candidate = null;
        }
        if (owner.gameObject.transform.position.y > 0) {
          // 重新选择
          if (owner.m_card.cardPhase.onlySelf) {
            owner.m_card.candidate = owner.m_card.owner;
          } else {
            owner.m_card.candidate = BattleManager.GetNearestEnemy(owner.gameObject.transform.position);
          }
          owner.m_card.candidate.SetSelectedEnable(true);
        }
      }

      public override void OnExit(int nextStateId) {
        if (owner.m_card.candidate != null) {
          owner.m_card.candidate.SetSelectedEnable(false);
          if (nextStateId != (int)BattleCardState.PLAYING) {
            owner.m_card.candidate = null;
          }
        }
      }
    }

    private class Playing : StateBase<UIBattleCard> {
      public Playing(UIBattleCard owner) : base((int)BattleCardState.PLAYING, owner) { }
    }
    
    private class Removing : StateBase<UIBattleCard> {
      private float m_speed;
      private AnimationCurve m_curve;
      private Vector2 m_originAnchoredPos;
      
      public Removing(UIBattleCard owner) : base((int)BattleCardState.REMOVING, owner) { }

      public override void OnEnter(int lastStateId) {
        m_speed = speed;
        owner.transform.SetParent(CardFactory.GetFolder(owner.m_card.cardHeapType));
        m_originAnchoredPos = owner.anchoredPosition;
        if (lastStateId == (int)BattleCardState.PLAYING) {
          int random = Random.Range(1, CARD_MOVE_CURVE_COUNT + 1);
          m_curve = ResourceManager.Load<AnimationCurve>(string.Format(CARD_MOVE_CURVE, random));
        } else {
          m_curve = ResourceManager.Load<AnimationCurve>(StringMap.COMMON_CURVE);
        }
      }

      public override void OnTrick() {
        if (owner.anchoredPosition.magnitude < MIN_DIST) {
          owner.transform.localScale = smallScale;
          owner.anchoredPosition = owner.m_targetAnchorPos;
          owner.transform.SetAngleZ(owner.m_targetAngle);
          stateMachine.SwitchState((int)BattleCardState.OUT_HAND);
        } else {
          owner.transform.localScale = Vector3.Lerp(owner.transform.localScale, smallScale, m_speed);
          Vector2 anchoredPosition = owner.anchoredPosition;
          anchoredPosition.x = Mathf.Lerp(anchoredPosition.x, owner.m_targetAnchorPos.x, m_speed);
          float t = (anchoredPosition.x - m_originAnchoredPos.x) / (owner.m_targetAnchorPos.x - m_originAnchoredPos.x);
          anchoredPosition.y = MathUtil.Calculate(m_curve, m_originAnchoredPos.y, owner.m_targetAnchorPos.y, t);
          owner.anchoredPosition = anchoredPosition;
          owner.transform.SetAngleZ(Mathf.LerpAngle(owner.transform.eulerAngles.z, owner.m_targetAngle, m_speed));
          m_speed += speedIncrement;
        }
      }
    }

    private class CardStateMachine : StateMachine {
      public CardStateMachine(UIBattleCard battleCard) {
        RegisterState(new Adding(battleCard));
        RegisterState(new InHand(battleCard));
        RegisterState(new Playing(battleCard));
        RegisterState(new OutHand(battleCard));
        RegisterState(new Dragging(battleCard));
        RegisterState(new Removing(battleCard));

        SwitchState((int)BattleCardState.OUT_HAND);
      }
    }

    #endregion

    private Card m_card;
    private Func<bool> m_onUseCard;
    private float m_targetAngle;
    private int m_targetSiblingIndex;
    private Vector2 m_targetAnchorPos;
    private PointerEventData m_eventData;
    public StateMachine stateMachine { private set; get; }
    private Vector2 anchoredPosition {
      get => m_card.rectTransform.anchoredPosition;
      set => m_card.rectTransform.anchoredPosition = value;
    }
    private static float speed => 0.5f * Time.deltaTime;
    private static float speedIncrement => 0.1f * speed;
    private static Vector3 smallScale => 0.3f * Vector3.one;
    private static Vector3 bigScale => 1.2f * Vector3.one;
    private const string SELECT_SOUND = "SOTE_SFX_CardSelect_v2";
    private const float MIN_DIST = 1.0f;
    private const float SPEED_SCALE = 10.0f;
    private const int CARD_MOVE_CURVE_COUNT = 5;
    private const string CARD_MOVE_CURVE = "card_move_curve{0}";

    public UIBattleCard Init(Card card, Func<bool> onUseCard) {
      m_card = card;
      m_onUseCard = onUseCard;
      stateMachine = new CardStateMachine(this);
      
      return this;
    }

    public void SetTarget(float targetAngle, int targetSiblingIndex, Vector2 targetAnchorPos) {
      m_targetAngle = targetAngle;
      m_targetAnchorPos = targetAnchorPos;
      m_targetSiblingIndex = targetSiblingIndex;
    }

    public void SetTargetSiblingIndex(int targetSiblingIndex) {
      m_targetSiblingIndex = targetSiblingIndex;
    }

    private void Update() {
      stateMachine.OnTrick();
    }

    public void OnPointerDown(PointerEventData eventData) {
      if (m_eventData != null) {
        return;
      }

      if (_CanDrag()) {
        m_eventData = eventData;
        stateMachine.SwitchState((int)BattleCardState.DRAGGING);
        AudioManager.PlaySound(SELECT_SOUND);
      }
    }

    public void OnPointerUp(PointerEventData eventData) {
      if (m_eventData != eventData) {
        return;
      }
      
      m_eventData = null;
      if (GetCurrentState() == BattleCardState.DRAGGING) {
        if (m_onUseCard == null || !m_onUseCard()) {
          stateMachine.SwitchState((int)BattleCardState.IN_HAND);
        } else {
          stateMachine.SwitchState((int)BattleCardState.PLAYING);
        }
      }
    }

    public BattleCardState GetCurrentState() {
      return (BattleCardState)stateMachine.currentState.stateId;
    }

    private bool _CanDrag() {
      return stateMachine.currentState.stateId == (int)BattleCardState.IN_HAND ||
             stateMachine.currentState.stateId == (int)BattleCardState.ADDING;
    }
  }
}