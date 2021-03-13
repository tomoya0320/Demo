using UnityEditor;
using UnityEngine;

namespace Yume.Editor {
  public class UICreateEditor {
    #region UITemplate
    private const string UI_TEMPLATE = "UITemplate";

    [MenuItem("GameObject/UI/" + UI_TEMPLATE)]
    public static void CreateUITemplate() {
      foreach (var transform in Selection.transforms) {
        GameObject gameObject = Object.Instantiate(Resources.Load<GameObject>(UI_TEMPLATE), transform);
        gameObject.name = UI_TEMPLATE;
        Selection.activeGameObject = gameObject;
      }
    }
    #endregion

    #region TextAutoScroll
    private const string TEXT_AUTO_SCROLL = "TextAutoScroll";

    [MenuItem("GameObject/UI/" + TEXT_AUTO_SCROLL)]
    public static void CreateTextAutoScroll() {
      foreach (var transform in Selection.transforms) {
        GameObject gameObject = Object.Instantiate(Resources.Load<GameObject>(TEXT_AUTO_SCROLL), transform);
        gameObject.name = TEXT_AUTO_SCROLL;
        Selection.activeGameObject = gameObject;
      }
    }
    #endregion

    #region YButton
    private const string BUTTON = "YButton";

    [MenuItem("GameObject/UI/" + BUTTON)]
    public static void CreateYButton() {
      foreach (var transform in Selection.transforms) {
        GameObject gameObject = Object.Instantiate(Resources.Load<GameObject>(BUTTON), transform);
        gameObject.name = BUTTON;
        Selection.activeGameObject = gameObject;
      }
    }
    #endregion
  }
}