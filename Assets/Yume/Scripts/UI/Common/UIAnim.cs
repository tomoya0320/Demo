using UnityEngine;
using Yume.String;

namespace Yume.UI {
  [ExecuteInEditMode, RequireComponent(typeof(Animation))]
  public class UIAnim : UIDisplay {
    private Animation m_animation;

    protected override bool isTweenPlaying {
      get {
        return m_animation.isPlaying;
      }
    }

    private void Awake() {
      m_animation = GetComponent<Animation>();
      m_animation.playAutomatically = false;
    }

    public override void Close() {
      if (isTweenPlaying) {
        return;
      }

      m_animation.Play(StringMap.UI_CLOSE_ANIM);
    }

    public override void Open() {
      if (isTweenPlaying) {
        return;
      }

      m_animation.Play(StringMap.UI_OPEN_ANIM);
    }
  }
}