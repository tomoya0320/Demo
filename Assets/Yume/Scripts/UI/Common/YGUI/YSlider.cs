using UnityEngine;
using UnityEngine.UI;

namespace Yume.UI {
  [ExecuteInEditMode]
  public class YSlider : Slider {
    protected override void Awake() {
      base.Awake();
      
      this.SetDefaultState();
    }
  }
}