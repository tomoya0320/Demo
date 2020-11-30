using UnityEngine;
using UnityEngine.UI;
using Yume.Server;

namespace Yume.UI {
  public class SplashAnim : MonoBehaviour {
    [SerializeField]
    private float _fadeTime = 0.5f;
    [SerializeField]
    private Image _image;

    private void _FadeBlackMask(Sprite sprite) {
      UIController.instance.FadeBlackMask(_fadeTime, () => {
        _image.sprite = sprite;
      });
    }

    private void _UpdateAssets() {
      UpdateAssetsController.instance.CheckGameAssets();
    }
  }
}