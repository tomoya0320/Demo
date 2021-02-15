using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace Yume.UI {
  public struct Message {
    public enum Style {
      [Description("slide")]
      SLIDE,
      [Description("fade")]
      FADE,
      [Description("center")]
      CENTER,
    }

    public Style style;
    public string description;
    public Sprite img;
    public float duration;
    public bool allowDuplicate;

    public Message(Style style, string description, Sprite img = null, float duration = 2.0f, bool allowDuplicate = true) {
      this.style = style;
      this.description = description;
      this.duration = duration;
      this.img = img;
      this.allowDuplicate = allowDuplicate;
    }
  }

  public static class MessageManager {
    private static string s_showingMessage;
    private static Queue<Message> s_messageQueue = new Queue<Message>();

    public static void AddMessage(Message message) {
      if (!message.allowDuplicate && message.description == s_showingMessage) {
        return;
      }

      s_messageQueue.Enqueue(message);
      if (string.IsNullOrEmpty(s_showingMessage)) {
        ShowMessage();
      }
    }

    public static void ShowMessage() {
      if (s_messageQueue.Count > 0) {
        var message = s_messageQueue.Dequeue();
        s_showingMessage = message.description;
        var uiName = $"message_{message.style.GetDescription()}";
        Action action = () => CoroutineHelper.instance.DelayInvoke(
          message.duration,
          () => UIManager.CloseOne(uiName, ShowMessage)
        );

        UIManager.OpenOne(uiName, action, message);
      } else {
        s_showingMessage = null;
      }
    }
  }
}