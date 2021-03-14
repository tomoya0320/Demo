using System;

namespace Yume.UI {
  public class UIGlobalFade : UIBase {
    public override void Open(Action callback = null, params object[] args) {
      // 屏蔽回退响应
      UIManager.activeBack--;

      // 配合引导需要先调用这个方法
      Action action = () => UIManager.CloseOne(name);
      callback = action + callback;

      base.Open(callback, args);
    }

    public override void Close(Action callback = null) {
      // 恢复回退响应
      callback += () => UIManager.activeBack++;

      base.Close(callback);
    }
  }
}