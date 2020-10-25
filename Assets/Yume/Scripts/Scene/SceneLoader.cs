using UnityEngine.SceneManagement;
using Yume.UI;

namespace Yume.Scene {
  public class SceneLoader {
    public static void GoToNextScene(string nextSceneName, float fadeTime = 0.3f, LoadSceneMode mode = LoadSceneMode.Single) {
      if (string.IsNullOrEmpty(nextSceneName)) {
        return;
      }

      UIController.instance.FadeBlackMask(true, fadeTime, () => {
        UIController.instance.sceneLoadingMaskActive = true;
        UIController.instance.FadeBlackMask(false, fadeTime, () => {
          SceneManager.LoadSceneAsync(nextSceneName, mode).completed += ao => {
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