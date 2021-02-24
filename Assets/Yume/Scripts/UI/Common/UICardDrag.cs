using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Yume.UI {
  public class UICardDrag : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler {
    private Vector3 m_originPos;
    private Vector3 m_clickDownPos;
    private Action m_onClickUp;
    private Func<bool> m_condition;
    private int m_originSiblingIndex;

    public void Init(Action onClickUp, Func<bool> condition) {
      m_onClickUp = onClickUp;
      m_condition = condition;
    }
    
    public void OnPointerDown(PointerEventData eventData) {
      m_originSiblingIndex = transform.GetSiblingIndex();
      transform.SetAsLastSibling();
      m_originPos = transform.position;
      m_clickDownPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

    public void OnPointerUp(PointerEventData eventData) {
      if (m_condition?.Invoke() ?? false) {
        m_onClickUp?.Invoke();
      } else {
        transform.SetSiblingIndex(m_originSiblingIndex);
        transform.position = m_originPos;
      }
    }

    public void OnDrag(PointerEventData eventData) {
      transform.position = m_originPos + (Camera.main.ScreenToWorldPoint(Input.mousePosition) - m_clickDownPos);
    }
  }
}