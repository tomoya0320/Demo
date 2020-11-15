using UnityEngine;
using UnityEngine.UI;
using Yume.Server;

namespace Yume.UI {
  [RequireComponent(typeof(Image), typeof(Animator))]
  public class SplashAnim : MonoBehaviour {
    [SerializeField]
    private float _fadeTime = 0.5f;
    private Image m_image;

    private void Awake() {
      m_image = GetComponent<Image>();
      UIController.instance.activeEscapeKey = false;
    }

    private void _FadeInBlackMask(Sprite sprite) {
      UIController.instance.FadeBlackMask(true, _fadeTime, () => {
        UIController.instance.FadeBlackMask(false, _fadeTime);
        m_image.sprite = sprite;
      });
    }

    private void _UpdateAssets() {
      UIController.instance.activeEscapeKey = true;
      UpdateAssetsController.instance.CheckGameAssets();
    }
  }
}