using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Yume.RedDotSystem;
using Object = UnityEngine.Object;

namespace Yume.UI {
  public class UIMail : UIGaussianBlurBg {
    private class UIMailTerm : UIChildBase, IScrollTerm<MailData> {
      private MailData m_mail;
      private Text m_titleText;
      private Button m_clickButton;
      private Text m_dateDeltaText;
      private Image m_attachmentImg;
      private GameObject m_selected;
      private UIMailGroup m_mailGroup;
      private UIRedDot m_mailRedDot;
      private MailTemplate m_mailTemplate;
      public bool isValid => m_mail != null;

      public UIMailTerm(GameObject root, UIMailGroup mailGroup) : base(root) {
        m_selected = GetGameObject("Selected");
        m_titleText = GetComponent<Text>("TitleText");
        m_clickButton = GetComponent<Button>("ClickButton");
        m_dateDeltaText = GetComponent<Text>("DateDeltaText");
        m_attachmentImg = GetComponent<Image>("AttachmentImage");

        m_mailGroup = mailGroup;
        m_mailRedDot = new UIRedDot(RedDotTreeType.MAIN_MAIL, m_mail?.id, 
          MainMailRedDotTree.MAIN_MAIL, m_clickButton.transform.Find(StringMap.UI_RED_DOT));

        _AddListener();
      }

      private void _AddListener() {
        m_clickButton.onClick.AddListener(() => {
          m_mailGroup.SwitchMail(m_mail);
        });
      }

      public void UpdateScrollTerm(MailData mail) {
        m_mail = mail;

        m_mailRedDot.Update(m_mail?.id);
        gameObject.SetActiveEx(m_mail != null);
        m_selected.SetActiveEx(m_mail == m_mailGroup.selectedMail);

        if (m_mail != null) {
          m_mailTemplate = ResourceManager.Load<MailTemplate>(m_mail.id);
          m_titleText.text = m_mailTemplate.title;
          m_attachmentImg.enabled = m_mailTemplate.attachments.Length > 0;
          m_dateDeltaText.text = MathUtil.CalDateDeltaFromNow(m_mail.ticks);
        }
      }

      public override void Destroy() {
        m_mailRedDot.Destroy();
      }

      public IScrollTerm<MailData> Clone() {
        var clone = Object.Instantiate(gameObject, gameObject.GetParent());
        clone.name = gameObject.name;
        return new UIMailTerm(clone, m_mailGroup);
      }
    }

    private class UIMailGroup : UIChildBase, IScrollGroup<MailData, UIMailTerm> {
      private Text m_dateText;
      private Text m_contentText;
      private YButton m_getButton;
      private Text m_signatureText;
      private Button m_getAllButton;
      private Button m_deleteAllButton;
      private GameObject m_noMailPanel;
      private GameObject m_mailContentPanel;

      private List<MailData> m_mails;
      private MailData m_selectedMail;
      private MailTemplate m_selectedMailTemplate;
      private ScrollRect m_mailScrollRect;
      public List<MailData> dataList => m_mails;
      private ObjectContainer[] m_attachmentObjects;
      public MailData selectedMail => m_selectedMail;
      public List<UIMailTerm> viewList => m_mailTermList;
      private UIScrollView<MailData, UIMailTerm> m_mailScrollView;
      private List<UIMailTerm> m_mailTermList = new List<UIMailTerm>();

      public UIMailGroup(GameObject root) : base(root) {
        m_dateText = GetComponent<Text>("DateText");
        m_noMailPanel = GetGameObject("NoMailPanel");
        m_getButton = GetComponent<YButton>("GetButton");
        m_contentText = GetComponent<Text>("ContentText");
        m_signatureText = GetComponent<Text>("SignatureText");
        m_getAllButton = GetComponent<Button>("GetAllButton");
        m_mailContentPanel = GetGameObject("MailContentPanel");
        m_deleteAllButton = GetComponent<Button>("DeleteAllButton");
        m_mailScrollRect = GetComponent<ScrollRect>("MailScrollRect");
        m_attachmentObjects = GetGameObject("Attachments").GetComponentsInChildren<ObjectContainer>(true);
        
        m_mails = DataManager.playerMails;
        
        for (int i = 0; i < m_mailScrollRect.content.childCount; i++) {
          m_mailTermList.Add(new UIMailTerm(m_mailScrollRect.content.GetChild(i).gameObject, this));
        }

        m_mailScrollView = new UIScrollView<MailData, UIMailTerm>(m_mailScrollRect, this);
        
        _AddListener();
      }

      public override void OnOpen() {
        m_mails.Sort((left, right) => string.CompareOrdinal(left.id, right.id));
        m_mailScrollView.ResetNormalizedPos();
        SwitchMail(m_mails.First());
        _RefreshMailView();
      }

      private void _AddListener() {
        m_getButton.onClick.AddListener(_GetAttachment);
        m_getAllButton.onClick.AddListener(_GetAllAttachment);
        m_deleteAllButton.onClick.AddListener(_DeleteAllMails);
      }

      public void SwitchMail(MailData mail) {
        if (m_selectedMail != mail) {
          m_selectedMail = mail;
          m_selectedMailTemplate = ResourceManager.Load<MailTemplate>(mail.id);
          _UpdateMailContent();
          m_mailScrollView.UpdateView(false);

          // 更新附件
          List<ItemArgs> attachments = new List<ItemArgs>(m_selectedMailTemplate.attachments);
          for (int i = 0; i < m_attachmentObjects.Length; i++) {
            var attachmentObject = m_attachmentObjects[i];
            if (i < attachments.Count) {
              attachmentObject.gameObject.SetActiveEx(true);
              attachmentObject.GetComponent<Image>("icon").SetSpriteAsync(ResourceManager.Load<ItemTemplate>(attachments[i].id).iconFileName);
              attachmentObject.GetComponent<Text>("num").text = attachments[i].count.ToString();
            } else {
              attachmentObject.gameObject.SetActiveEx(false);
            }
          }
        }
      }

      private void _UpdateMailContent() {
        m_mailContentPanel.SetActiveEx(true);
        m_signatureText.text = m_selectedMailTemplate.signature;
        m_dateText.text = DateTime.FromBinary(m_selectedMail.ticks).ToShortDateString();
        m_contentText.text = string.Format(m_selectedMailTemplate.content, DataManager.playerName);

        _CheckGetButton();
      }

      public override void OnUpdate() {
        m_mailScrollView.OnUpdate();
      }

      public override void Destroy() {
        foreach (var mailTerm in m_mailTermList) {
          mailTerm.Destroy();
        }
      }

      private void _CheckGetButton() {
        m_getButton.interactable = !m_selectedMail.isReceived;
        m_getButton.textComponent.text = m_selectedMail.isReceived ? StringMap.GOT : StringMap.CLICK_GET;
        m_getButton.image.color = m_selectedMail.isReceived ? Colors.DISABLE_BUTTON : m_getButton.colors.normalColor;
      }

      private void _CheckGetAllButton() {
        foreach (MailData mail in m_mails) {
          if (!mail.isReceived) {
            m_getAllButton.interactable = true;
            m_getAllButton.image.color = m_getAllButton.colors.normalColor;
            return;
          }
        }

        m_getAllButton.interactable = false;
        m_getAllButton.image.color = Colors.DISABLE_BUTTON;
      }

      private void _GetAttachment() {
        if (!m_selectedMail.isReceived) {
          DataManager.GetAttachment(m_selectedMail);

          _CheckGetButton();
          _CheckGetAllButton();
        }
      }

      private void _GetAllAttachment() {
        foreach (var mail in m_mails.FindAll(mail => !mail.isReceived)) {
          DataManager.GetAttachment(mail);
          if (mail == m_selectedMail) {
            _CheckGetButton();
          }
        }
        m_getAllButton.interactable = false;
        m_getAllButton.image.color = Colors.DISABLE_BUTTON;
      }

      private void _DeleteAllMails() {
        m_selectedMail = null;
        foreach (var mail in m_mails.FindAll(mail => mail.isReceived)) {
          DataManager.DeleteMail(mail);
        }

        _RefreshMailView();
      }

      private void _CheckContentPanel() {
        m_noMailPanel.SetActiveEx(m_mails.Count == 0);
        m_mailContentPanel.SetActiveEx(m_selectedMail != null);
      }

      private void _RefreshMailView() {
        _CheckGetAllButton();
        _CheckContentPanel();
        m_mailScrollView.UpdateView();
      }
    }

    private UIMailGroup m_mailGroup;

    public override void OnInit(UITemplate uiTemplate, GameObject gameObject) {
      base.OnInit(uiTemplate, gameObject);
      
      m_mailGroup = new UIMailGroup(GetGameObject("MailGroup"));

      _AddListener();
    }

    public override void Open(Action callback = null, params object[] args) {
      m_mailGroup.OnOpen();

      base.Open(callback, args);
    }

    public override void OnUpdate() {
      m_mailGroup.OnUpdate();
    }

    private void _AddListener() {
      GetComponent<Button>("close").onClick.AddListener(() => UIManager.CloseOne(name));
    }

    public override void Destroy() {
      base.Destroy();
      m_mailGroup.Destroy();
    }
  }
}