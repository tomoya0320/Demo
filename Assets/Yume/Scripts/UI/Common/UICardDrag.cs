using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using Yume.Audio;

namespace Yume.UI {
  public class UICardDrag : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {
    private Tween m_tween;
    private bool m_dragging;
    private Vector3 m_originPos;
    private Vector3 m_clickDownPos;
    private Func<bool> m_condition;
    private Func<bool> m_onUseCard;
    private Action m_onDraggingCard;
    private int m_originSiblingIndex;
    public int enable { private set; get; }
    public bool interactable => enable > 0 && m_tween == null;
    private const string SELECT_SOUND = "SOTE_SFX_CardSelect_v2";

    public UICardDrag Init(Action onDraggingCard, Func<bool> onUseCard, Func<bool> condition) {
      m_onUseCard = onUseCard;
      m_condition = condition;
      m_onDraggingCard = onDraggingCard;
      return this;
    }

    public void SetEnable(bool enable) {
      this.enable += enable ? 1 : -1;
    }

    public void OnPointerDown(PointerEventData eventData) {
      if (!interactable || m_condition == null || !m_condition()) {
        return;
      }

      m_dragging = true;
      m_originPos = transform.position;
      m_originSiblingIndex = transform.GetSiblingIndex();
      transform.SetAsLastSibling();
      Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
      m_clickDownPos = mousePosition;
      
      AudioManager.PlaySound(SELECT_SOUND);
    }

    public void OnPointerUp(PointerEventData eventData) {
      if (!m_dragging) {
        return;
      }

      m_dragging = false;
      if (m_onUseCard == null || !m_onUseCard()) {
        m_tween = transform.DOMove(m_originPos, 0.2f).OnComplete(
          () => {
            transform.SetSiblingIndex(m_originSiblingIndex);
            m_tween = null;
          }
        );
      }
    }

    private void Update() {
      if (m_dragging) {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.position = m_originPos + (mousePosition - m_clickDownPos);
        m_onDraggingCard?.Invoke();
      }
    }
  }
}