using UnityEngine;
using UnityEngine.UI;

namespace Yume.UI {
  [ExecuteInEditMode]
  public class YInputField : InputField {
    protected override void Awake() {
      base.Awake();

      this.SetDefaultState();
    }
  }
}