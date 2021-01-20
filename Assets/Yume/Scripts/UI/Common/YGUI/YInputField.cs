using UnityEngine;
using UnityEngine.UI;

namespace Yume.UI {
  [ExecuteInEditMode]
  public class YInputField : InputField {
    protected override void Awake() {
      base.Awake();

      ColorBlock newColors = colors;
      newColors.disabledColor = Color.white;
      colors = newColors;
    }
  }
}