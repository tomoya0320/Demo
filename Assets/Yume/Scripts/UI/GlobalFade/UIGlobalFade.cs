using System;

namespace Yume.UI {
  public class UIGlobalFade : UIBase {
    public override void Open(Action callback = null, params object[] args) {
      callback += () => UIManager.CloseOne(name);

      base.Open(callback, args);
    }
  }
}