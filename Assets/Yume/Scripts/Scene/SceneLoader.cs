using UnityEngine.SceneManagement;
using Yume.UI;

namespace Yume.Scene {
  public class SceneLoader {
    public static void GoToNextScene(string nextSceneName, float fadeTime = 0.3f) {
      if (string.IsNullOrEmpty(nextSceneName)) {
        return;
      }

      UIController.instance.FadeBlackMask(true, fadeTime, () => {
        UIController.instance.sceneLoadingMaskActive = true;
        UIController.instance.FadeBlackMask(false, fadeTime, () => {
          SceneManager.LoadSceneAsync(nextSceneName).completed += ao => {
            UIController.instance.FadeBlackMask(true, fadeTime, () => {
              UIController.instance.sceneLoadingMaskActive = false;
              UIController.instance.FadeBlackMask(false, fadeTime);
            });
          };
        });
      });
    }
  }
}