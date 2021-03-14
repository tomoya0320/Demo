using System;
using UnityEngine;
using UnityEngine.UI;

namespace Yume.UI {
  public class UIAVGHistory : UIBase {
    private Text m_dialogueText;
    private ScrollRect m_scrollRect;

    public override void OnInit(UITemplate uiTemplate, GameObject gameObject) {
      base.OnInit(uiTemplate, gameObject);

      m_dialogueText = GetComponent<Text>("dialogue");
      m_scrollRect = GetComponent<ScrollRect>("ScrollRect");

      m_scrollRect.vertical = true;
      m_scrollRect.horizontal = false;

      _AddListener();
    }

    public override void Open(Action callback = null, params object[] args) {
      m_dialogueText.text = args[0] as string;
      m_scrollRect.verticalNormalizedPosition = 0;

      base.Open(callback, args);
    }

    private void _AddListener() {
      GetComponent<Button>("close").onClick.AddListener(() => UIManager.CloseOne(name));
    }
  }
}