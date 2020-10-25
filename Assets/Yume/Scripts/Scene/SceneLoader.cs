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

    public static void GoToAVGScene(string avgName, float fadeTime = 0.3f) {
      if (string.IsNullOrEmpty(avgName)) {
        return;
      }

      UIController.instance.FadeBlackMask(true, fadeTime, () => {
        UIController.instance.sceneLoadingBGActive = true;
        UIController.instance.FadeBlackMask(false, fadeTime, () => {
          SceneManager.LoadSceneAsync(StringMap.AVG, LoadSceneMode.Additive).completed += ao => {
            UIController.instance.FadeBlackMask(true, fadeTime, () => {
              AVGController.instance.StartAVG(avgName);
              UIController.instance.sceneLoadingBGActive = false;
              UIController.instance.FadeBlackMask(false, fadeTime);
            });
          };
        });
      });
    }
  }
}