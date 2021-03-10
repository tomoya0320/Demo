using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using Yume.Audio;

namespace Yume.UI {
  [RequireComponent(typeof(RectTransform))]
  public class UICardDrag : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {
    private int m_enable;
    private Tween m_tween;
    private bool m_dragging;
    private Vector3 m_originPos;
    private Vector3 m_clickDownPos;
    private Func<bool> m_onUseCard;
    private Action m_onDraggingCard;
    private int m_originSiblingIndex;
    private Vector2 m_resetAnchorPos;
    public bool dragging => m_dragging;
    private PointerEventData m_eventData;
    private RectTransform m_rectTransform;
    public bool interactable => m_enable > 0 && m_tween == null;
    private const float SCALE_TIME = 0.2f;
    private static Vector3 s_draggingScale = 1.2f * Vector3.one;
    private const string SELECT_SOUND = "SOTE_SFX_CardSelect_v2";

    private void Awake() {
      m_rectTransform = GetComponent<RectTransform>();
    }

    public UICardDrag Init(Action onDraggingCard, Func<bool> onUseCard) {
      m_onUseCard = onUseCard;
      m_onDraggingCard = onDraggingCard;
      return this;
    }

    public void SetEnable(bool enable) {
      m_enable += enable ? 1 : -1;
      m_dragging &= interactable;
    }

    public void SetResetAnchorPos(Vector2 resetAnchorPos) {
      m_resetAnchorPos = resetAnchorPos;
    }

    public void OnPointerDown(PointerEventData eventData) {
      if (!interactable || m_dragging) {
        return;
      }

      m_dragging = true;
      m_originPos = m_rectTransform.position;
      m_originSiblingIndex = m_rectTransform.GetSiblingIndex();
      m_rectTransform.SetAsLastSibling();

      m_eventData = eventData;
      Vector3 mousePosition = m_eventData.position;
      // 坐标变换
      mousePosition = Camera.main.ScreenToWorldPoint(mousePosition);
      m_clickDownPos = mousePosition;

      AudioManager.PlaySound(SELECT_SOUND);
    }

    public void OnPointerUp(PointerEventData eventData) {
      if (!interactable || !m_dragging || m_eventData != eventData) {
        return;
      }

      m_dragging = false;
      m_eventData = null;
      if (m_onUseCard == null || !m_onUseCard()) {
        m_tween = m_rectTransform.DOAnchorPos(m_resetAnchorPos, 0.2f).OnComplete(
          () => {
            m_rectTransform.SetSiblingIndex(m_originSiblingIndex);
            m_tween = null;
          }
        );
      }
    }

    private void Update() {
      if (interactable) {
        if (m_dragging) {
          Vector3 mousePosition = m_eventData.position;
          // 坐标变换
          mousePosition = Camera.main.ScreenToWorldPoint(mousePosition);
          m_rectTransform.position = m_originPos + (mousePosition - m_clickDownPos);
          m_rectTransform.localScale = Vector3.Lerp(m_rectTransform.localScale, s_draggingScale, SCALE_TIME);
          m_onDraggingCard?.Invoke();
        } else {
          m_rectTransform.localScale = Vector3.Lerp(m_rectTransform.localScale, Vector3.one, SCALE_TIME);
        }
      }
    }
  }
}