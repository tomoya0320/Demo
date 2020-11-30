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

      UIController.instance.activeEscapeKey = false;

      UIController.instance.FadeBlackMask(fadeTime, () => {
        UIController.instance.sceneLoadingBGActive = true;
      }, () => {
        SceneManager.LoadSceneAsync(nextSceneName).completed += ao => {
          UIController.instance.FadeBlackMask(fadeTime, () => {
            UIController.instance.sceneLoadingBGActive = false;
          }, () => {
            UIController.instance.activeEscapeKey = true;
          });
        };
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