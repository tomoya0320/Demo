using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace Yume.UI {
  public class Message {
    public enum Style {
      [Description("slide")]
      SLIDE,
      [Description("fade")]
      FADE
    }

    public Style style;
    public string description;
    public Sprite img;
    public float duration;
    public bool isShowImg;

    public Message(Style style, string description, Sprite img, float duration = 2.0f) {
      this.style = style;
      this.description = description;
      this.duration = duration;
      this.img = img;
      isShowImg = true;
    }
    public Message(Style style, string description, float duration = 2.0f) {
      this.style = style;
      this.description = description;
      this.duration = duration;
      isShowImg = false;
    }
  }
  public class MessageManager : Singleton<MessageManager> {
    private bool m_isMessageShowing;
    private Queue<Message> m_messageQueue = new Queue<Message>();

    public void AddMessage(Message message) {
      m_messageQueue.Enqueue(message);
      if (!m_isMessageShowing) {
        m_isMessageShowing = true;
        ShowMessage();
      }
    }

    public void ShowMessage() {
      if (m_messageQueue.Count > 0) {
        var message = m_messageQueue.Dequeue();
        UIManager.instance.OpenOne($"message_{message.style.GetDescription()}", null, message);
      } else {
        m_isMessageShowing = false;
      }
    }
  }
}