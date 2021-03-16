using UnityEngine;
using UnityEngine.UI;
using Yume.Broadcast;
using Yume.RedDotSystem;
using EventType = Yume.Broadcast.EventType;

namespace Yume.UI {
  public class UIMain : UIBase {
    private Button m_bag;
    private Button m_task;
    private Button m_card;
    private Button m_char;
    private Button m_mail; 
    private Button m_team; 
    private Button m_setting;
    private Button m_battle;
    
    private Text m_lvText;
    private Image m_expImg;
    private Text m_nameText;
    private Text m_goldNumText;
    private Text m_diamondNumText;

    private UIRedDot m_taskRedDot;
    private UIRedDot m_mailRedDot;

    public override void OnInit(UITemplate uiTemplate, GameObject gameObject) {
      base.OnInit(uiTemplate, gameObject);
      
      m_lvText = GetComponent<Text>("LvText");
      m_expImg = GetComponent<Image>("ExpImage");
      m_nameText = GetComponent<Text>("NameText");
      m_goldNumText = GetComponent<Text>("GoldNumText");
      m_diamondNumText = GetComponent<Text>("DiamondNumText");

      m_bag = GetComponent<Button>("Bag");
      m_task = GetComponent<Button>("Task");
      m_card = GetComponent<Button>("Card");
      m_char = GetComponent<Button>("Char");
      m_mail = GetComponent<Button>("Mail");
      m_team = GetComponent<Button>("Team");
      m_setting = GetComponent<Button>("Setting");
      m_battle = GetComponent<Button>("Battle");

      m_taskRedDot = new UIRedDot(RedDotTreeType.MAIN_TASK, MainTaskRedDotTree.MAIN_TASK,
        null, m_task.transform.Find(StringMap.UI_RED_DOT));

      m_mailRedDot = new UIRedDot(RedDotTreeType.MAIN_MAIL, MainMailRedDotTree.MAIN_MAIL,
        null, m_mail.transform.Find(StringMap.UI_RED_DOT));

      _AddListener();
      _UpdateUserInfo();

      EventCenter.AddListener(EventType.UPDATE_USER_INFO, _UpdateUserInfo);
    }

    private void _AddListener() {
      m_bag.onClick.AddListener(() => UIManager.OpenOne("bag"));
      m_task.onClick.AddListener(() => UIManager.OpenOne("task"));
      m_card.onClick.AddListener(() => UIManager.OpenOne("card"));
      m_char.onClick.AddListener(() => UIManager.OpenOne("char"));
      m_mail.onClick.AddListener(() => UIManager.OpenOne("mail"));
      m_team.onClick.AddListener(() => UIManager.OpenOne("team"));
      m_setting.onClick.AddListener(() => UIManager.OpenOne("setting"));
      m_battle.onClick.AddListener(() => UIManager.OpenOne("battle"));
    }

    private void _UpdateUserInfo() {
      m_goldNumText.text = DataManager.GetItemCount(StringMap.GOLD_ITEM_ID).ToString();
      m_diamondNumText.text = DataManager.GetItemCount(StringMap.DIAMOND_ITEM_ID).ToString();

      m_lvText.text = $"{DataManager.playerLv + 1}";
      m_nameText.text = DataManager.playerName;

      float needExp = MathUtil.CalNeedUserExp(DataManager.playerLv);
      m_expImg.fillAmount = DataManager.GetItemCount(StringMap.EXP_ITEM_ID) / needExp;
    }

    public override void Destroy() {
      m_taskRedDot.Destroy();
      m_mailRedDot.Destroy();

      EventCenter.RemoveListener(EventType.UPDATE_USER_INFO, _UpdateUserInfo);
    }
  }
}