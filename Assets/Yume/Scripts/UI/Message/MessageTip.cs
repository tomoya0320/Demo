using UnityEngine;
using UnityEngine.UI;
using System;

namespace Yume.UI {
  public class UIMessageTip : UIBase {
    private Image m_image;
    private Message m_message;
    private Text m_description;
    private RectTransform m_textRect;

    public override void OnInit(UIData uiData, GameObject root) {
      base.OnInit(uiData, root);
      m_image = GetComponent<Image>("image");
      m_description = GetComponent<Text>("description");
      m_textRect = m_description.GetComponent<RectTransform>();
    }

    public override void Open(Action onOpenStart = null, Action onOpenEnd = null, params object[] args) {
      m_message = args[0] as Message;

      if (m_message.isShowImg) {
        if (m_message.img != null) {
          m_image.sprite = m_message.img;
        }
      } else {
        m_image.enabled = false;
      }

      m_description.text = m_message.description;

      Vector2 size = m_textRect.rect.size;
      // height保持不变
      m_textRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
      // 动态设置width
      m_textRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,
         Mathf.Clamp(m_description.preferredWidth, 300, 1500));

      onOpenEnd = onOpenEnd.AddAction(() => Close(null, _ShowNextMessage));

      base.Open(onOpenStart, onOpenEnd, args);
    }

    private void _ShowNextMessage() {
      MessageManager.instance.RemoveMessage(m_message);
      MessageManager.instance.ShowMessage();
    }
  }
}