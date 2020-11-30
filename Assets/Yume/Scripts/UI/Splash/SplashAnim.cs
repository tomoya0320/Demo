using UnityEngine;
using UnityEngine.UI;
using Yume.Server;

namespace Yume.UI {
  [RequireComponent(typeof(Image), typeof(Animator))]
  public class SplashAnim : MonoBehaviour {
    [SerializeField]
    private float _fadeTime = 0.5f;
    private Image m_image;

    private void Start() {
      m_image = GetComponent<Image>();
      UIController.instance.activeEscapeKey = false;
    }

    private void _FadeBlackMask(Sprite sprite) {
      UIController.instance.FadeBlackMask(_fadeTime, () => {
        m_image.sprite = sprite;
      });
    }

    private void _UpdateAssets() {
      UIController.instance.activeEscapeKey = true;
      UpdateAssetsController.instance.CheckGameAssets();
    }
  }
}