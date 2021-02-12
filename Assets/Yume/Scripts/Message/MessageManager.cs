using System;
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
      FADE,
      [Description("center")]
      CENTER,
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
  public static class MessageManager {
    private static bool s_isMessageShowing;
    private static Queue<Message> s_messageQueue = new Queue<Message>();

    public static void AddMessage(Message message) {
      s_messageQueue.Enqueue(message);
      if (!s_isMessageShowing) {
        s_isMessageShowing = true;
        ShowMessage();
      }
    }

    public static void ShowMessage() {
      if (s_messageQueue.Count > 0) {
        var message = s_messageQueue.Dequeue();
        var uiName = $"message_{message.style.GetDescription()}";
        Action action = () => CoroutineHelper.instance.DelayInvoke(message.duration,
         () => UIManager.CloseOne(uiName, ShowMessage));

        UIManager.OpenOne(uiName, action, message);
      } else {
        s_isMessageShowing = false;
      }
    }
  }
}