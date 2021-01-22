using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Yume.Broadcast;
using Yume.Data;
using Yume.RedPointSystem;

namespace Yume.UI {
  public class UIMail : UIBase {
    private class UIMailTerm : UIChildBase, IScrollTerm<MailData> {
      private MailData m_mail;
      private Text m_titleText;
      private Button m_clickButton;
      private Text m_dateDeltaText;
      private Image m_attachmentImg;
      private GameObject m_selected;
      private UIMailGroup m_mailGroup;
      private UIRedPoint m_mailRedPoint;
      public bool isValid => m_mail != null;

      public UIMailTerm(UIMailGroup mailGroup, GameObject root) : base(root) {
        m_selected = GetGameObject("Selected");
        m_titleText = GetComponent<Text>("TitleText");
        m_clickButton = GetComponent<Button>("ClickButton");
        m_dateDeltaText = GetComponent<Text>("DateDeltaText");
        m_attachmentImg = GetComponent<Image>("AttachmentImage");

        m_mailGroup = mailGroup;
        var mailRedPointRoot = root.FindGo(StringMap.UI_RED_POINT);
        m_mailRedPoint = new UIRedPoint(RedPointTreeType.MAIN_MAIL, m_mail?.id, MainMailRedPointTree.MAIN_MAIL, mailRedPointRoot);

        _AddListener();
      }

      private void _AddListener() {
        m_clickButton.onClick.AddListener(() => {
          m_mailGroup.SwitchMail(m_mail);
        });
      }

      public void UpdateScrollTerm(MailData mail) {
        m_mail = mail;

        m_mailRedPoint.Update(m_mail?.id);
        gameObject.SetActiveEx(m_mail != null);
        m_selected.SetActiveEx(m_mail == m_mailGroup.selectedMail);

        if (m_mail != null) {
          m_titleText.text = m_mail.title;
          m_attachmentImg.enabled = m_mail.attachment.Count > 0;
          m_dateDeltaText.text = MathUtil.CalDateDeltaFromNow(m_mail.timeStamp);
        }
      }

      public override void Destroy() {
        m_mailRedPoint.Destroy();
      }
    }

    private class UIMailGroup : UIChildBase {
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
      private ScrollRect m_mailScrollRect;
      private ScrollRect m_attachmentScrollRect;
      public MailData selectedMail => m_selectedMail;
      private UIScrollView<MailData, UIMailTerm> m_mailScrollView;
      private List<UIMailTerm> m_mailTermList = new List<UIMailTerm>();
      private List<ItemData> m_selectedAttachment = new List<ItemData>();
      private UIScrollView<ItemData, UIShortItemInfo> m_attachmentScrollView;
      private List<UIShortItemInfo> m_attachmentTermList = new List<UIShortItemInfo>();

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
        m_attachmentScrollRect = GetComponent<ScrollRect>("AttachmentScrollRect");

        m_mails = DataManager.userMails;

        for (int i = 0; i < m_mailScrollRect.content.childCount; i++) {
          m_mailTermList.Add(new UIMailTerm(this, m_mailScrollRect.content.GetChild(i).gameObject));
        }

        for (int i = 0; i < m_attachmentScrollRect.content.childCount; i++) {
          m_attachmentTermList.Add(new UIShortItemInfo(m_attachmentScrollRect.content.GetChild(i).gameObject));
        }

        m_mailScrollView = new UIScrollView<MailData, UIMailTerm>(m_mailScrollRect, m_mails, m_mailTermList);
        m_attachmentScrollView = new UIScrollView<ItemData, UIShortItemInfo>(m_attachmentScrollRect, m_selectedAttachment, m_attachmentTermList);

        _AddListener();
        _CheckGetAllButton();
        _CheckContentPanel();

        EventCenter.AddListener(Broadcast.EventType.REFRESH_MAIL_VIEW, _RefreshMailView);
      }

      private void _AddListener() {
        m_getButton.onClick.AddListener(_GetAttachment);
        m_getAllButton.onClick.AddListener(_GetAllAttachment);
        m_deleteAllButton.onClick.AddListener(_DeleteAllMails);
      }

      public void SwitchMail(MailData mail) {
        if (m_selectedMail != mail) {
          m_selectedMail = mail;
          _UpdateMailContent();
          m_mailScrollView.UpdateView(false);
          m_selectedAttachment.Clear();
          m_selectedAttachment.AddRange(m_selectedMail.attachment);
          m_attachmentScrollView.UpdateView();
        }
      }

      private void _UpdateMailContent() {
        m_mailContentPanel.SetActiveEx(true);
        m_signatureText.text = m_selectedMail.signature;
        m_dateText.text = MathUtil.GetDateTime(m_selectedMail.timeStamp).ToShortDateString();
        m_contentText.text = string.Format(m_selectedMail.content, DataManager.userName);

        _CheckGetButton();
      }

      public override void OnUpdate() {
        m_mailScrollView.OnUpdate();
        m_attachmentScrollView.OnUpdate();
      }

      public override void Destroy() {
        foreach (var mailTerm in m_mailTermList) {
          mailTerm.Destroy();
        }
        EventCenter.RemoveListener(Broadcast.EventType.REFRESH_MAIL_VIEW, _RefreshMailView);
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
      }

      private void _RefreshMailView() {
        _CheckGetAllButton();
        _CheckContentPanel();
        m_mailScrollView.UpdateView();
      }

      private void _CheckContentPanel() {
        m_noMailPanel.SetActiveEx(m_mails.Count == 0);
        m_mailContentPanel.SetActiveEx(m_selectedMail != null);
      }
    }

    private UIMailGroup m_mailGroup;

    public override void OnInit(UIData uiData, GameObject root) {
      base.OnInit(uiData, root);
      
      m_mailGroup = new UIMailGroup(GetGameObject("MailGroup"));

      _AddListener();
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