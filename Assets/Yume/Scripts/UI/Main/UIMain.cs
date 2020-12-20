using System;
using UnityEngine;
using UnityEngine.UI;

namespace Yume.UI {
  public class UIMain : UIBase {
    private Button m_bag;
    private Button m_task;
    private Button m_card;
    private Button m_mail; 
    private Button m_setting;

    public override void OnInit(UIData uiData, GameObject root) {
      base.OnInit(uiData, root);
      m_bag = GetComponent<Button>("Bag");
      m_task = GetComponent<Button>("Task");
      m_card = GetComponent<Button>("Card");
      m_mail = GetComponent<Button>("Mail");
      m_setting = GetComponent<Button>("Setting");

      _AddListener();
    }

    private void _AddListener() {
      m_bag.onClick.AddListener(() => UIManager.instance.OpenOne("bag"));
      m_task.onClick.AddListener(() => UIManager.instance.OpenOne("task"));
      m_card.onClick.AddListener(() => UIManager.instance.OpenOne("card"));
      m_mail.onClick.AddListener(() => UIManager.instance.OpenOne("mail"));
      m_setting.onClick.AddListener(() => UIManager.instance.OpenOne("setting"));
    }

    public override void Open(Action onOpenStart = null, Action onOpenEnd = null, params object[] args) {
      onOpenStart = onOpenStart.AddAction(() => root.SetActive(true));

      base.Open(onOpenStart, onOpenEnd, args);
    }

    public override void Close(Action onCloseStart = null, Action onCloseEnd = null, params object[] args) {
      onCloseEnd = onCloseEnd.AddAction(() => root.SetActive(false));

      base.Close(onCloseStart, onCloseEnd, args);
    }
  }
}