using UnityEngine;
using UnityEngine.UI;
using Yume.Broadcast;
using Yume.DB;
using Yume.Math;
using Yume.RedPointSystem;
using Yume.String;
using EventType = Yume.Broadcast.EventType;

namespace Yume.UI {
  public class UIMain : UIBase {
    private Button m_bag;
    private Button m_task;
    private Button m_card;
    private Button m_mail; 
    private Button m_setting;

    private Text m_apText;
    private Text m_lvText;
    private Image m_expImg;
    private Text m_nameText;
    private Text m_goldNumText;
    private Text m_diamondNumText;

    private UIRedPoint m_bagRedPoint;
    private UIRedPoint m_taskRedPoint;
    private UIRedPoint m_mailRedPoint;

    public override void OnInit(UIData uiData, GameObject root) {
      base.OnInit(uiData, root);

      m_apText = GetComponent<Text>("ApText");
      m_lvText = GetComponent<Text>("LvText");
      m_expImg = GetComponent<Image>("ExpImage");
      m_nameText = GetComponent<Text>("NameText");
      m_goldNumText = GetComponent<Text>("GoldNumText");
      m_diamondNumText = GetComponent<Text>("DiamondNumText");

      m_bag = GetComponent<Button>("Bag");
      m_task = GetComponent<Button>("Task");
      m_card = GetComponent<Button>("Card");
      m_mail = GetComponent<Button>("Mail");
      m_setting = GetComponent<Button>("Setting");

      var bagRedPointRoot = m_bag.gameObject.FindGo(StringMap.UI_RED_POINT);
      m_bagRedPoint = new UIRedPoint(RedPointTreeType.MAIN_ITEM, MainItemRedPointTree.MAIN_ITEM, null, bagRedPointRoot);
      var taskRedPointRoot = m_task.gameObject.FindGo(StringMap.UI_RED_POINT);
      m_taskRedPoint = new UIRedPoint(RedPointTreeType.MAIN_TASK, MainTaskRedPointTree.MAIN_TASK, null, taskRedPointRoot);
      var mailRedPointRoot = m_mail.gameObject.FindGo(StringMap.UI_RED_POINT);
      m_mailRedPoint = new UIRedPoint(RedPointTreeType.MAIN_MAIL, MainMailRedPointTree.MAIN_MAIL, null, mailRedPointRoot);

      _AddListener();
      _UpdateUserInfo();

      EventCenter.AddListener(EventType.UPDATE_USER_INFO, _UpdateUserInfo);
    }

    private void _AddListener() {
      m_bag.onClick.AddListener(() => UIManager.instance.OpenOne("bag"));
      m_task.onClick.AddListener(() => UIManager.instance.OpenOne("task"));
      m_card.onClick.AddListener(() => UIManager.instance.OpenOne("card"));
      m_mail.onClick.AddListener(() => UIManager.instance.OpenOne("mail"));
      m_setting.onClick.AddListener(() => UIManager.instance.OpenOne("setting"));
    }

    private void _UpdateUserInfo() {
      m_goldNumText.text = DBManager.instance.GetItemDBNum(StringMap.GOLD_ID).ToString();
      m_diamondNumText.text = DBManager.instance.GetItemDBNum(StringMap.DIAMOND_ID).ToString();

      m_apText.text = DBManager.instance.userAp + "/" + DBManager.instance.userMaxAp;
      m_lvText.text = DBManager.instance.userLv.ToString();
      m_nameText.text = DBManager.instance.userName;

      float needEXP = MathUtil.CalNeededExp(DBManager.instance.userLv);
      m_expImg.fillAmount = DBManager.instance.userExp / needEXP;
    }

    public override void Destroy() {
      m_bagRedPoint.Destroy();
      m_taskRedPoint.Destroy();
      m_mailRedPoint.Destroy();

      EventCenter.RemoveListener(EventType.UPDATE_USER_INFO, _UpdateUserInfo);
    }
  }
}