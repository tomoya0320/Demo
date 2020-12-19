using System;
using UnityEngine;
using UnityEngine.UI;

namespace Yume.UI {
  public class UIBlackMask : UIBase {
    private Image m_mask;

    public override void OnInit(UIData uiData, GameObject root) {
      base.OnInit(uiData, root);
      m_mask = GetComponent<Image>("Mask");
    }

    public override void Open(Action onOpenStart = null, Action onOpenEnd = null, params object[] args) {
      onOpenStart = onOpenStart.AddAction(() => m_mask.enabled = true);
      onOpenEnd = onOpenEnd.AddAction(() => UIManager.instance.CloseOne("blackMask"));

      base.Open(onOpenStart, onOpenEnd, args);
    }

    public override void Close(Action onCloseStart = null, Action onCloseEnd = null, params object[] args) {
      onCloseEnd = onCloseEnd.AddAction(() => m_mask.enabled = false);

      base.Close(onCloseStart, onCloseEnd, args);
    }
  }
}