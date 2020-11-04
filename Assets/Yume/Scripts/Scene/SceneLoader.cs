using UnityEngine.SceneManagement;
using Yume.AVG;
using Yume.String;
using Yume.UI;

namespace Yume.Scene {
  public class SceneLoader {
    public static void GoToNextScene(string nextSceneName, float fadeTime = 0.3f) {
      if (string.IsNullOrEmpty(nextSceneName)) {
        return;
      }

      UIController.instance.ClearBackStack();

      UIController.instance.FadeBlackMask(true, fadeTime, () => {
        UIController.instance.sceneLoadingBGActive = true;
        UIController.instance.FadeBlackMask(false, fadeTime, () => {
          SceneManager.LoadSceneAsync(nextSceneName).completed += ao => {
            UIController.instance.FadeBlackMask(true, fadeTime, () => {
              UIController.instance.sceneLoadingBGActive = false;
              UIController.instance.FadeBlackMask(false, fadeTime);
            });
          };
        });
      });
    }

    public static void GoToAVGScene(string avgName) {
      if (string.IsNullOrEmpty(avgName)) {
        return;
      }

      UIController.instance.activeEscapeKey = false;

      SceneManager.LoadSceneAsync(StringMap.AVG, LoadSceneMode.Additive).completed += ao => {
        AVGController.instance.StartAVG(avgName);
      };
    }
  }
}