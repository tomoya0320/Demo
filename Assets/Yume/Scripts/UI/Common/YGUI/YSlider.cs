using UnityEngine;
using UnityEngine.UI;

namespace Yume.UI {
  [ExecuteInEditMode]
  public class YSlider : Slider {
    protected override void Awake() {
      base.Awake();
      ColorBlock newColors = colors;
      newColors.normalColor = Color.white;
      newColors.highlightedColor = Color.white;
      newColors.selectedColor = Color.white;
      newColors.disabledColor = Color.white;
      colors = newColors;
    }
  }
}