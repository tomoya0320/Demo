using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using Yume.Tool;

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

    public Message(Style style, string description, Sprite img = null, float duration = 2.0f) {
      this.style = style;
      this.description = description;
      this.duration = duration;
      this.img = img;
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
        var uiName = $"message_{message.style.GetDescription()}";
        void action() => CoroutineHelper.instance.RunCoroutine(
          _DelayInvoke(message.duration, () => UIManager.instance.CloseOne(uiName, ShowMessage)));

        UIManager.instance.OpenOne(uiName, action, message);
      } else {
        m_isMessageShowing = false;
      }
    }

    private IEnumerator _DelayInvoke(float time, Action callback) {
      yield return new WaitForSeconds(time);
      callback?.Invoke();
    }
  }
}