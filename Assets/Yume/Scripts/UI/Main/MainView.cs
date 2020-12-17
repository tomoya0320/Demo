using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Yume.UI {
  public class MainView : UIBase {
    public override void Close(params object[] args) {
      base.Close(args);
      root.SetActive(false);
    }

    public override void Open(params object[] args) {
      root.SetActive(true);
      base.Open(args);
    }
  }
}