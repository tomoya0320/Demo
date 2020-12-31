using System;
using UnityEngine;
using UnityEngine.UI;

namespace Yume.UI {
  public class UIMask : UIBase {
    private Image m_mask;

    public override void OnInit(UIData uiData, GameObject root) {
      base.OnInit(uiData, root);
      m_mask = GetComponent<Image>("Mask");
    }

    public override void Open(Action callback = null, params object[] args) {
      m_mask.enabled = true;
      callback += () => UIManager.instance.CloseOne(name);
      base.Open(callback, args);
    }

    public override void Close(Action callback = null, params object[] args) {
      callback += () => m_mask.enabled = false;
      base.Close(callback, args);
    }
  }
}