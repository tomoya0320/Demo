using System;
using Coffee.UIExtensions;
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
        owner.m_targetScale = Vector3.one;
        owner.transform.SetParent(CardFactory.GetFolder(CardHeapType.HAND));
        
        m_speed = speed;
      }

      public override void OnTrick() {
        if (Vector2.Distance(owner.m_targetAnchorPos, owner.anchoredPosition) < MIN_DIST) {
          stateMachine.SwitchState((int)BattleCardState.IN_HAND);
        } else {
          owner._DoTransformTween(m_speed);
        }
        m_speed += speedIncrement;
      }
    }

    private class InHand : StateBase<UIBattleCard> {
      public InHand(UIBattleCard owner) : base((int)BattleCardState.IN_HAND, owner) { }

      public override void OnEnter(int lastStateId) {
        owner.m_targetScale = Vector3.one;
        owner.transform.SetParent(CardFactory.GetFolder(CardHeapType.HAND));
      }

      public override void OnTrick() {
        owner._DoTransformTween(SPEED_SCALE * speed);
        owner.transform.SetSiblingIndex(owner.m_targetSiblingIndex);
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
        owner.anchoredPosition = owner.m_eventData.position - 0.5f * ScreenUtil.screenSize;
        // 目标选中的逻辑层
        Unit oldCandidate = owner.m_card.candidate;
        if (owner.gameObject.transform.position.y > 0) {
          if (owner.m_card.cardPhase.onlySelf) {
            owner.m_card.candidate = owner.m_card.owner;
          } else {
            owner.m_card.candidate = BattleManager.GetNearestEnemy(owner.gameObject.transform.position);
          }
        } else {
          owner.m_card.candidate = null;
        }
        // 目标选中的表现层
        if (oldCandidate != owner.m_card.candidate) {
          oldCandidate?.SetSelectedEnable(false);
          owner.m_card.candidate?.SetSelectedEnable(true);
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
        owner.m_targetScale = smallScale;
        owner.transform.SetParent(CardFactory.GetFolder(owner.m_card.cardHeapType));
        
        m_speed = speed;
        m_originAnchoredPos = owner.anchoredPosition;
        if (lastStateId == (int)BattleCardState.PLAYING) {
          AnimationCurve[] cardMoveCurves = ResourceManager.Load<AnimationCurve[]>(CARD_MOVE_CURVES);
          m_curve = cardMoveCurves[Random.Range(0, cardMoveCurves.Length)];
        } else {
          m_curve = ResourceManager.Load<AnimationCurve>(StringMap.COMMON_CURVE);
        }
      }

      public override void OnTrick() {
        switch (owner.m_card.cardHeapType) {
          case CardHeapType.DISCARD:
            if (Vector2.Distance(owner.m_targetAnchorPos, owner.anchoredPosition) < MIN_DIST) {
              owner.ResetTransform();
              stateMachine.SwitchState((int)BattleCardState.OUT_HAND);
            } else {
              owner._DoTransformTween(m_curve, m_originAnchoredPos, m_speed);
            }
            break;
          case CardHeapType.CONSUME:
            if (MathUtil.Approximately(owner.m_dissolve.effectFactor, 1.0f)) {
              owner.ResetTransform();
              stateMachine.SwitchState((int)BattleCardState.OUT_HAND);
            } else {
              owner.m_dissolve.effectFactor = Mathf.Lerp(owner.m_dissolve.effectFactor, 1.0f, m_speed);
            }
            break;
        }
        m_speed += speedIncrement;
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

        currentState = states[(int)BattleCardState.OUT_HAND];
        currentState.OnEnter(-1);
      }
    }

    #endregion

    private Card m_card;
    private float m_targetAngle;
    private UIDissolve m_dissolve;
    private Func<bool> m_onUseCard;
    private int m_targetSiblingIndex;
    private CanvasGroup m_canvasGroup;
    private Vector2 m_targetAnchorPos;
    private StateMachine m_stateMachine;
    private PointerEventData m_eventData;
    private Vector3 m_targetScale = Vector3.one;
    public BattleCardState curState => (BattleCardState)m_stateMachine.currentState.stateId;
    private Vector2 anchoredPosition {
      get => m_card.rectTransform.anchoredPosition;
      set => m_card.rectTransform.anchoredPosition = value;
    }
    private static float speed => 0.6f * Time.deltaTime;
    private static float speedIncrement => 0.1f * speed;
    public static Vector3 smallScale => 0.3f * Vector3.one;
    private static Vector3 bigScale => 1.2f * Vector3.one;
    private const string SELECT_SOUND = "SOTE_SFX_CardSelect_v2";
    private const float MIN_DIST = 1.0f;
    private const float SPEED_SCALE = 10.0f;
    private const string CARD_MOVE_CURVES = "cardMoveCurves";

    public UIBattleCard Init(Card card, Func<bool> onUseCard) {
      m_card = card;
      m_onUseCard = onUseCard;
      m_stateMachine = new CardStateMachine(this);
      
      m_dissolve = GetComponentInChildren<UIDissolve>();
      m_canvasGroup = GetComponent<CanvasGroup>();
      m_canvasGroup.blocksRaycasts = true;

      return this;
    }

    public void SetTarget(float targetAngle, int targetSiblingIndex, Vector2 targetAnchorPos) {
      m_targetAngle = targetAngle;
      m_targetAnchorPos = targetAnchorPos;
      m_targetSiblingIndex = targetSiblingIndex;
    }

    public void SetTargetAngle(float targetAngle) {
      m_targetAngle = targetAngle;
    }   
    public void SetTargetAnchorPos(Vector2 targetAnchorPos) {
      m_targetAnchorPos = targetAnchorPos;
    }
    
    public void SetTargetSiblingIndex(int targetSiblingIndex) {
      m_targetSiblingIndex = targetSiblingIndex;
    }

    public void OnUpdate() {
      m_stateMachine.OnTrick();
    }

    public void OnPointerDown(PointerEventData eventData) {
      if (m_eventData != null) {
        return;
      }

      if (_CanDrag()) {
        m_eventData = eventData;
        m_stateMachine.SwitchState((int)BattleCardState.DRAGGING);
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
          m_stateMachine.SwitchState((int)BattleCardState.IN_HAND);
        } else {
          m_stateMachine.SwitchState((int)BattleCardState.PLAYING);
        }
      }
    }

    public void SwitchState(BattleCardState state, bool force = false) {
      m_stateMachine.SwitchState((int)state, force);
    }

    public BattleCardState GetCurrentState() {
      return (BattleCardState)m_stateMachine.currentState.stateId;
    }

    private bool _CanDrag() {
      return m_stateMachine.currentState.stateId == (int)BattleCardState.IN_HAND ||
             m_stateMachine.currentState.stateId == (int)BattleCardState.ADDING;
    }

    private void _DoTransformTween(float speed) {
      transform.localScale = Vector3.Lerp(transform.localScale, m_targetScale, speed);
      anchoredPosition = Vector2.Lerp(anchoredPosition, m_targetAnchorPos, speed);
      transform.SetAngleZ(Mathf.LerpAngle(transform.eulerAngles.z, m_targetAngle, speed));
    }    
    
    private void _DoTransformTween(AnimationCurve curve, Vector2 originAnchoredPos, float speed) {
      transform.localScale = Vector3.Lerp(transform.localScale, m_targetScale, speed);
      Vector2 newAnchoredPos = anchoredPosition;
      newAnchoredPos.x = Mathf.Lerp(newAnchoredPos.x, m_targetAnchorPos.x, speed);
      float t = (newAnchoredPos.x - originAnchoredPos.x) / (m_targetAnchorPos.x - originAnchoredPos.x);
      newAnchoredPos.y = MathUtil.Calculate(curve, originAnchoredPos.y, m_targetAnchorPos.y, t);
      anchoredPosition = newAnchoredPos;
      transform.SetAngleZ(Mathf.LerpAngle(transform.eulerAngles.z, m_targetAngle, speed));
    }

    public void ResetTransform() {
      transform.SetParentEx(CardFactory.GetFolder(m_card.cardHeapType));
      anchoredPosition = m_targetAnchorPos;
      transform.localScale = m_targetScale;
      transform.SetAngleZ(m_targetAngle);
      m_dissolve.effectFactor = 0;
    }
  }
}