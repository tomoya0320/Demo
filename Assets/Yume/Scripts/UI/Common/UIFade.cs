using DG.Tweening;
using UnityEngine;

namespace Yume.UI {
  [RequireComponent(typeof(CanvasGroup))]
  public class UIFade : UIDisplay {
    [SerializeField]
    private float _fadeTime = 0.1f;
    private CanvasGroup m_canvasGroup;
    private Tween m_tween;
    protected override bool isTweenPlaying {
      get {
        return m_tween?.IsPlaying() ?? false;
      }
    }

    private void Awake() {
      m_canvasGroup = GetComponent<CanvasGroup>();
    }

    public override void Open() {
      if (isTweenPlaying) {
        return;
      }

      if (useUIStack) {
        UIController.instance.AddToBackStack(this);
      }

      m_canvasGroup.interactable = true;
      m_canvasGroup.blocksRaycasts = true;
      m_tween = m_canvasGroup.DOFade(1.0f, _fadeTime);
    }

    public override void Close() {
      if (isTweenPlaying) {
        return;
      }

      m_tween = m_canvasGroup.DOFade(0, _fadeTime).OnComplete(() => {
        m_canvasGroup.interactable = false;
        m_canvasGroup.blocksRaycasts = false;

        if (useUIStack) {
          UIController.instance.RemoveFromBackStack(this);
        }
      })
    ;
    }
  }
}