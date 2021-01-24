using System;
using UnityEngine;
using UnityEngine.UI;

namespace Yume.UI {
  public class UIGlobalCircleBlackFade : UIGlobalFade {
    private Image m_image;

    public override void OnInit(UIData uiData, GameObject root) {
      base.OnInit(uiData, root);

      m_image = GetComponent<Image>("image");
    }

    public override void Open(Action callback = null, params object[] args) {
      Vector2 clickPos = (Vector2)args[0];
      Color color = m_image.color;
      color.r = clickPos.x;
      color.g = clickPos.y;
      m_image.color = color;

      base.Open(callback, args);
    }
  }
}