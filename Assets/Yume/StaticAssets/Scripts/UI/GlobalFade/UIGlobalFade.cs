using System;

namespace Yume.UI {
  public class UIGlobalFade : UIBase {
    public override void Open(Action callback = null, params object[] args) {
      // 屏蔽回退响应
      UIManager.activeBack--;

      callback += () => UIManager.CloseOne(name);

      base.Open(callback, args);
    }

    public override void Close(Action callback = null, params object[] args) {
      // 恢复回退响应
      callback += () => UIManager.activeBack++;

      base.Close(callback, args);
    }
  }
}