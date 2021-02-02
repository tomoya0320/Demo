using UnityEngine;

namespace Yume.UI {
  [ExecuteInEditMode]
  public class UIFullScrenn : MonoBehaviour {
    private RectTransform m_rectTransform;
    public static readonly Vector2 referenceResolution = new Vector2(1920, 1080);

    private void Awake() {
      m_rectTransform = GetComponent<RectTransform>();
    }

    private void Start() {
      _Fit();
    }

    private void _Fit() {
      float scaleFactorX = Screen.width / referenceResolution.x;
      float scaleFactorY = Screen.height / referenceResolution.y;

      Vector2 newSize = referenceResolution;

      if (scaleFactorX > scaleFactorY) {
        newSize *= scaleFactorX / scaleFactorY;
      } else {
        newSize *= scaleFactorY / scaleFactorX;
      }

      m_rectTransform.sizeDelta = newSize;
    }

#if UNITY_EDITOR
    private void Update() {
      _Fit();
    }
#endif
  }
}