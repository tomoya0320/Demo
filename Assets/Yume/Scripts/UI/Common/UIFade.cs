using DG.Tweening;
using UnityEngine;

namespace Yume.UI {
  [RequireComponent(typeof(CanvasGroup))]
  public class UIFade : MonoBehaviour, IUIDisplay {
    [SerializeField]
    private bool _addToUIStack = true;
    [SerializeField]
    private float _fadeTime = 0.1f;
    private CanvasGroup m_canvasGroup;
    private CanvasGroup canvasGroup {
      get {
        if (m_canvasGroup == null) {
          m_canvasGroup = GetComponent<CanvasGroup>();
          m_canvasGroup.interactable = false;
        }
        return m_canvasGroup;
      }
    }
    private Tween m_tween;


    public void Open() {
      if (_addToUIStack) {
        UIController.instance.AddToBackStack(this);
      }
      if (m_tween != null) {
        m_tween.Complete();
      }
      canvasGroup.alpha = 0;
      gameObject.SetActive(true);
      m_tween = canvasGroup.DOFade(1.0f, _fadeTime).OnComplete(() => {
        canvasGroup.interactable = true;
        m_tween = null;
      });
    }

    public void Close() {
      if (m_tween != null) {
        m_tween.Complete();
      }
      canvasGroup.alpha = 1.0f;
      canvasGroup.interactable = false;
      m_tween = canvasGroup.DOFade(0, _fadeTime).OnComplete(() => {
        gameObject.SetActive(false);
        m_tween = null;
      });
      if (_addToUIStack) {
        UIController.instance.RemoveFromBackStack(this);
      }
    }
  }
}