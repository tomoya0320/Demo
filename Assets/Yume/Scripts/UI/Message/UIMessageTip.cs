using UnityEngine;
using UnityEngine.UI;
using System;
using Yume.Data;

namespace Yume.UI {
  public class UIMessageTip : UIBase {
    private Message m_message;
    private Image m_image;
    private Text m_description;
    private RectTransform m_textRect;

    public override void OnInit(UITemplate uiTemplate, GameObject root) {
      base.OnInit(uiTemplate, root);
      m_image = GetComponent<Image>("image");
      m_description = GetComponent<Text>("description");
      m_textRect = m_description.GetComponent<RectTransform>();
    }

    public override void Open(Action callback = null, params object[] args) {
      m_message = args[0] as Message;

      m_image.sprite = m_message.img;
      m_image.enabled = m_image.sprite != null;

      m_description.text = m_message.description;

      Vector2 size = m_textRect.rect.size;
      // height保持不变
      m_textRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
      // 动态设置width
      m_textRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,
         Mathf.Clamp(m_description.preferredWidth, 300, 1500));

      base.Open(callback, args);
    }
  }
}