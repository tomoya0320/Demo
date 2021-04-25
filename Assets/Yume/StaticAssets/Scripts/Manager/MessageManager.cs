using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.UI;

namespace Yume {
  public struct Message {
    public enum Style {
      [Description("message_slide")]
      SLIDE,
      [Description("message_fade")]
      FADE,
      [Description("message_center")]
      CENTER,
    }

    public Style style;
    public string content;
    public string spriteName;
    public float duration;
    public bool allowDuplicate;

    public Message(Style style, string content, bool allowDuplicate = true, string spriteName = null, float duration = 2.0f) {
      this.style = style;
      this.content = content;
      this.duration = duration;
      this.spriteName = spriteName;
      this.allowDuplicate = allowDuplicate;
    }
  }

  // TODO:改进消息提示
  public static class MessageManager {
    private class UIMessage {
      private Message m_message;
      private Image m_image;
      private Text m_description;
      private Animation m_animation;
      private GameObject m_gameObject;
      private RectTransform m_textRect;

      public UIMessage(GameObject gameObject) {
        gameObject.SetActiveEx(false);

        m_gameObject = gameObject;
        m_animation = m_gameObject.GetComponent<Animation>();

        var objectContainer = m_gameObject.GetComponent<ObjectContainer>();
        m_image = objectContainer.GetComponent<Image>("image");
        m_description = objectContainer.GetComponent<Text>("description");
        m_textRect = m_description.GetComponent<RectTransform>();
      }

      public void Show(Message.Style style) {
        m_message = s_messageQueueDict[style].Peek();
        if (string.IsNullOrEmpty(m_message.spriteName)) {
          m_image.enabled = false;
        } else {
          m_image.enabled = true;
          m_image.SetSpriteAsync(m_message.spriteName);
        }

        m_description.text = m_message.content;
        m_textRect.SetSizeX(Mathf.Clamp(m_description.preferredWidth, 300, 1500));

        m_gameObject.SetActiveEx(true);
        CoroutineHelper.instance.PlayAnimCallback(m_animation.GetClip("open"), m_animation, _OnOpenCompleted);
      }

      private void _OnOpenCompleted() {
        CoroutineHelper.instance.DelayInvoke(m_message.duration, _OnStayCompleted);
      }

      private void _OnStayCompleted() {
        CoroutineHelper.instance.PlayAnimCallback(m_animation.GetClip("close"), m_animation, _OnCloseCompleted);
      }

      private void _OnCloseCompleted() {
        m_gameObject.SetActiveEx(false);
        s_messageQueueDict[m_message.style].Dequeue();
        // show next message
        if (s_messageQueueDict[m_message.style].Count > 0) {
          Show(m_message.style);
        }
      }
    }

    private static Dictionary<Message.Style, UIMessage> s_uiMessageDict;
    private static Dictionary<Message.Style, Queue<Message>> s_messageQueueDict;

    public static void Init() {
      // 因为这个初始化语句太长了就放这里了
      s_uiMessageDict = new Dictionary<Message.Style, UIMessage>();
      s_messageQueueDict = new Dictionary<Message.Style, Queue<Message>>();
      foreach (Message.Style style in Enum.GetValues(typeof(Message.Style))) {
        var gameObject = ObjectUtil.Instantiate(style.GetDescription(), UIManager.tipCanvas);
        s_uiMessageDict.Add(style, new UIMessage(gameObject));
        s_messageQueueDict.Add(style, new Queue<Message>());
      }
    }

    public static void AddMessage(Message message) {
      if (!message.allowDuplicate) {
        foreach (var item in s_messageQueueDict[message.style]) {
          if (item.content == message.content) {
            return;
          }
        }
      }
      s_messageQueueDict[message.style].Enqueue(message);
      if (s_messageQueueDict[message.style].Count == 1) {
        s_uiMessageDict[message.style].Show(message.style);
      }
    }
  }
}