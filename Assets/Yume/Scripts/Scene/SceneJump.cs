using UnityEngine;
using UnityEngine.SceneManagement;
using Yume.UI;

namespace Yume.Scene {
  public class SceneJump : MonoBehaviour {
    [SerializeField]
    private string _nextSceneName;
    [SerializeField]
    private float _fadeTime = 0.2f;

    public void GoToNextScene() {
      if (string.IsNullOrEmpty(_nextSceneName)) {
        return;
      }

      UIController.instance.FadeBlackMask(true, _fadeTime, () => {
        UIController.instance.sceneLoadingMaskActive = true;
        UIController.instance.FadeBlackMask(false, _fadeTime, () => {
          SceneManager.LoadSceneAsync(_nextSceneName).completed += ao => {
            UIController.instance.FadeBlackMask(true, _fadeTime, () => {
              UIController.instance.sceneLoadingMaskActive = false;
              UIController.instance.FadeBlackMask(false, _fadeTime);
            });
          };
        });
      });
    }
  }
}