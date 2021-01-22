using UnityEditor;
using UnityEngine;

namespace Yume.Editor {
  public class YTextAutoScrollEditor {
    private const string TEXT_AUTO_SCROLL = "TextAutoScroll";

    [MenuItem("GameObject/UI/" + TEXT_AUTO_SCROLL)]
    public static void Create() {
      foreach (var transform in Selection.transforms) {
        GameObject gameObject = Object.Instantiate(Resources.Load<GameObject>(TEXT_AUTO_SCROLL), transform);
        gameObject.name = TEXT_AUTO_SCROLL;
        Selection.activeGameObject = gameObject;
      }
    }
  }
}