using System;
using UnityEngine;
using UnityEngine.UI;

namespace Yume.UI {
  public class UIConfirmation : UIBase {
    private Text m_label;
    private YButton m_ok;
    private YButton m_cancel;

    public override void OnInit(UITemplate uiTemplate, GameObject gameObject) {
      base.OnInit(uiTemplate, gameObject);

      m_ok = GetComponent<YButton>("ok");
      m_label = GetComponent<Text>("label");
      m_cancel = GetComponent<YButton>("cancel");
    }

    public override void Open(Action callback = null, params object[] args) {
      m_label.text = (string)args[0];
      m_ok.textComponent.text = (string)args[1];
      m_cancel.textComponent.text = (string)args[2];

      m_ok.onClick.RemoveAllListeners();
      m_ok.onClick.AddListener(() => { UIManager.CloseOne(name, false, args[3] as Action); });
      m_cancel.onClick.RemoveAllListeners();
      m_cancel.onClick.AddListener(() => { UIManager.CloseOne(name, false, args[4] as Action); });

      base.Open(callback, args);
    }
  }
}