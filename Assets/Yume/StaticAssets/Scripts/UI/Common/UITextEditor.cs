using System;
using UnityEngine;
using UnityEngine.UI;

namespace Yume.UI {
  public class UITextEditor : UIBase {
    private InputField m_textInputField;
    private Action<string> m_onCompleted;

    public override void OnInit(UITemplate uiTemplate, GameObject gameObject) {
      base.OnInit(uiTemplate, gameObject);

      m_textInputField = GetComponent<InputField>("TextInputField");

      _AddListener();
    }

    public override void Open(Action callback = null, params object[] args) {
      base.Open(callback, args);

      m_textInputField.text = args[0].ToString();
      m_onCompleted = args[1] as Action<string>;
    }

    private void _AddListener() {
      GetComponent<Button>("ok").onClick.AddListener(() => {
        UIManager.CloseOne(name, () => {
          m_onCompleted?.Invoke(m_textInputField.text);
         });
      });
      GetComponent<Button>("cancel").onClick.AddListener(() => UIManager.CloseOne(name));
      GetComponent<Button>("CancelFull").onClick.AddListener(() => UIManager.CloseOne(name));
    }
  }
}