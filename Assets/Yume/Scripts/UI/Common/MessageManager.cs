using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Yume.Singleton;
using Yume.String;

namespace Yume.UI {
  public class Message {
    public enum Style {
      SLIDE, FADE
    }

    public Style style;
    public string description;
    public Sprite img;
    public float duration;
    public float delay;
    public bool isShowImg;

    public Message(Style style, string description, Sprite img, float duration = 0.5f, float delay = 1.0f) {
      this.style = style;
      this.description = description;
      this.duration = duration;
      this.delay = delay;
      this.img = img;
      isShowImg = true;
    }
    public Message(Style style, string description, float duration = 0.8f, float delay = 1.6f) {
      this.style = style;
      this.description = description;
      this.duration = duration;
      this.delay = delay;
      isShowImg = false;
    }
  }
  public class MessageManager {
    private GameObject m_tempGo;
    private bool m_isMessageShowing;
    private List<Message> m_messages;

    public MessageManager() {
      m_messages = new List<Message>();
    }

    public void AddMessage(Message message) {
      int index = m_messages.FindIndex(m => {
        return m.description == message.description;
      });
      if (index < 0) {
        m_messages.Add(message);
        if (!m_isMessageShowing) {
          m_isMessageShowing = true;
          ShowMessage();
        }
      }
    }

    public bool RemoveMessage(Message message) {
      return m_messages.Remove(message);
    }

    public async void ShowMessage() {
      if (m_messages.Count > 0) {
        int index = (int)m_messages[0].style;
        m_tempGo = await Addressables.InstantiateAsync(StringMap.MESSAGE + index, 
                                      UIController.instance.messageFolder).Task;
        MessageShow ms = m_tempGo.GetComponent<MessageShow>();
        ms.OnInit(m_messages[0]);
      } else {
        m_isMessageShowing = false;
      }
    }
  }
}