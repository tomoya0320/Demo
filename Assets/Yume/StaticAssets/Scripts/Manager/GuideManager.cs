using UnityEngine;
using UnityEngine.UI;

namespace Yume {
  public static class GuideManager {
    private static GameObject s_guidMask;
    private static Graphic s_guidMaskGraphic;
    
    public static void Init() {
      var guideMaskPrefab = Resources.Load<GameObject>("guide_mask");
      s_guidMask = Object.Instantiate(guideMaskPrefab, UIManager.guidelCanvas);
      s_guidMask.name = "guide_mask";
      s_guidMaskGraphic = s_guidMask.GetComponent<Graphic>();
      
      s_guidMask.SetActiveEx(false);
    }
  }
}