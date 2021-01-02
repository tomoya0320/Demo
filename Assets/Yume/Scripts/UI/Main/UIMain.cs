using System;
using UnityEngine;
using UnityEngine.UI;
using Yume.RedPointSystem;
using Yume.String;

namespace Yume.UI {
  public class UIMain : UIBase {
    private Button m_bag;
    private Button m_task;
    private Button m_card;
    private Button m_mail; 
    private Button m_setting;

    private UIRedPoint m_bagRedPoint;
    private UIRedPoint m_taskRedPoint;
    private UIRedPoint m_mailRedPoint;

    public override void OnInit(UIData uiData, GameObject root) {
      base.OnInit(uiData, root);
      m_bag = GetComponent<Button>("Bag");
      m_task = GetComponent<Button>("Task");
      m_card = GetComponent<Button>("Card");
      m_mail = GetComponent<Button>("Mail");
      m_setting = GetComponent<Button>("Setting");

      var bagRedPointGo = m_bag.gameObject.FindGo(StringMap.UI_RED_POINT);
      m_bagRedPoint = new UIRedPoint(RedPointTreeType.MAIN_ITEM, MainItemRedPointTree.MAIN_ITEM, null, bagRedPointGo);
      var taskRedPointGo = m_task.gameObject.FindGo(StringMap.UI_RED_POINT);
      m_taskRedPoint = new UIRedPoint(RedPointTreeType.MAIN_TASK, MainTaskRedPointTree.MAIN_TASK, null, taskRedPointGo);
      var mailRedPointGo = m_mail.gameObject.FindGo(StringMap.UI_RED_POINT);
      m_mailRedPoint = new UIRedPoint(RedPointTreeType.MAIN_MAIL, MainMailRedPointTree.MAIN_MAIL, null, mailRedPointGo);

      _AddListener();
    }

    private void _AddListener() {
      m_bag.onClick.AddListener(() => UIManager.instance.OpenOne("bag"));
      m_task.onClick.AddListener(() => UIManager.instance.OpenOne("task"));
      m_card.onClick.AddListener(() => UIManager.instance.OpenOne("card"));
      m_mail.onClick.AddListener(() => UIManager.instance.OpenOne("mail"));
      m_setting.onClick.AddListener(() => UIManager.instance.OpenOne("setting"));
    }

    public override void Destroy() {
      m_bagRedPoint.Destroy();
      m_taskRedPoint.Destroy();
      m_mailRedPoint.Destroy();
    }
  }
}