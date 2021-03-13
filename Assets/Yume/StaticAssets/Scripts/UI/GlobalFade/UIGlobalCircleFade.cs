using System;
using UnityEngine;
using UnityEngine.UI;
using Yume.Data;

namespace Yume.UI {
  public class UIGlobalCircleFade : UIGlobalFade {
    private Image m_image;

    public override void OnInit(UITemplate uiTemplate, GameObject gameObject) {
      base.OnInit(uiTemplate, gameObject);

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