using System;
using System.Collections;
using UnityEngine;
using Yume.String;

namespace Yume.UI {
  [ExecuteInEditMode, RequireComponent(typeof(Animation))]
  public class UIAnim : UIDisplay {
    private Animation m_animation;
    private bool m_isTweenPlaying;

    protected override bool isTweenPlaying => m_isTweenPlaying;

    private void Awake() {
      m_animation = GetComponent<Animation>();
      m_animation.playAutomatically = false;
    }

    public override void Open() {
      if (isTweenPlaying) {
        return;
      }

      if (useUIStack) {
        UIController.instance.AddToBackStack(this);
      }

      m_isTweenPlaying = true;//因为协程得等下一帧才会执行，下同
      StartCoroutine(_PlayAnimInternal(StringMap.UI_OPEN_ANIM));
    }

    public override void Close() {
      if (isTweenPlaying) {
        return;
      }

      m_isTweenPlaying = true;
      StartCoroutine(_PlayAnimInternal(StringMap.UI_CLOSE_ANIM, () => {
        if (useUIStack) {
          UIController.instance.RemoveFromBackStack(this);
        }
      }));
    }

    private IEnumerator _PlayAnimInternal(string name, Action onComplete = null) {
      m_animation.Play(name);
      while (m_animation.isPlaying) {
        yield return null;
      }
      m_isTweenPlaying = false;
      onComplete?.Invoke();
    }
  }
}