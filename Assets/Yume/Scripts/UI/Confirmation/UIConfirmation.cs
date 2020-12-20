using System;
using UnityEngine;
using UnityEngine.UI;

namespace Yume.UI {
  public class UIConfirmation : UIBase {
    private Text m_label;
    private YButton m_ok;
    private YButton m_cancel;

    public override void OnInit(UIData uiData, GameObject root) {
      base.OnInit(uiData, root);

      m_label = GetComponent<Text>("label");
      m_ok = GetComponent<YButton>("ok");
      m_cancel = GetComponent<YButton>("cancel");
    }

    public override void Open(Action onOpenStart = null, Action onOpenEnd = null, params object[] args) {
      m_label.text = (string)args[0];
      m_ok.textComponent.text = (string)args[1];
      m_cancel.textComponent.text = (string)args[2];

      m_ok.onClick.RemoveAllListeners();
      m_ok.onClick.AddListener(()=> {
        (args[3] as Action)?.Invoke();
        UIManager.instance.CloseOne(name);
      });
      m_cancel.onClick.RemoveAllListeners();
      m_cancel.onClick.AddListener(()=> {
        (args[4] as Action)?.Invoke();
        UIManager.instance.CloseOne(name);
      });

      base.Open(onOpenStart, onOpenEnd, args);
    }
  }
}