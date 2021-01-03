using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Yume.Broadcast;
using Yume.Battle;
using Yume.DB;
using Yume.MailSystem;
using Yume.Math;
using Yume.String;
using Yume.RedPointSystem;

namespace Yume.UI {
  public class UIMail : UIBase {
    private class UIMailTerm : UIChildBase, IScrollTerm<MailDB> {
      private MailDB m_mail;
      private Text m_titleText;
      private Image m_bottomImg;
      private Button m_clickButton;
      private Text m_dateDeltaText;
      private Image m_attachmentImg;
      private GameObject m_selected;
      private UIMailGroup m_mailGroup;

      public MailDB mail => m_mail;
      public Image bottomImg => m_bottomImg;
      public bool isValid => m_mail != null;

      private UIRedPoint m_mailRedPoint;

      public UIMailTerm(UIMailGroup mailGroup, GameObject root) : base(root) {
        m_mailGroup = mailGroup;
        m_selected = GetGameObject("Selected");
        m_titleText = GetComponent<Text>("TitleText");
        m_bottomImg = GetComponent<Image>("BottomImage");
        m_clickButton = GetComponent<Button>("ClickButton");
        m_dateDeltaText = GetComponent<Text>("DateDeltaText");
        m_attachmentImg = GetComponent<Image>("AttachmentImage");

        var mailRedPointRoot = root.FindGo(StringMap.UI_RED_POINT);
        m_mailRedPoint = new UIRedPoint(RedPointTreeType.MAIN_MAIL, m_mail?.id, MainMailRedPointTree.MAIN_MAIL, mailRedPointRoot);

        _AddListener();
      }

      private void _AddListener() {
        m_clickButton.onClick.AddListener(() => {
          m_mailGroup.SwitchMail(m_mail);
          m_mailGroup.mailScrollView.UpdateViewList();
        });
      }

      public void Update(MailDB data) {
        m_mail = data;
        if (m_mail == null) {
          gameObject.SetActiveEx(false);
        } else {
          gameObject.SetActiveEx(true);
          m_titleText.text = mail.title;
          m_attachmentImg.enabled = mail.attachment.Length > 0;
          m_selected.SetActiveEx(m_mail == m_mailGroup.selectedMail);
          m_dateDeltaText.text = MathUtil.CalDateDeltaFromNow(mail.dateTime);
        }
        m_mailRedPoint.Update(m_mail?.id);
      }

      public void Destroy() {
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

      private List<MailDB> m_mails;
      private MailDB m_selectedMail;
      public MailDB selectedMail => m_selectedMail;
      private GridLayoutGroup m_mailGLG;
      private ScrollRect m_mailScrollRect;
      private GridLayoutGroup m_attachmentGLG;
      private ScrollRect m_attachmentScrollRect;
      private UIScrollView<MailDB, UIMailTerm> m_mailScrollView;
      public UIScrollView<MailDB, UIMailTerm> mailScrollView => m_mailScrollView;
      private List<UIMailTerm> m_mailTermList = new List<UIMailTerm>();
      private UIScrollView<ItemDB, UIShortItemInfo> m_attachmentScrollView;
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

        m_mails = DBManager.instance.userMails;
        m_noMailPanel.SetActiveEx(m_mails.Count == 0);

        m_mailGLG = m_mailScrollRect.content.GetComponent<GridLayoutGroup>();
        m_attachmentGLG = m_attachmentScrollRect.content.GetComponent<GridLayoutGroup>();

        for (int i = 0; i < m_mailScrollRect.content.childCount; i++) {
          m_mailTermList.Add(new UIMailTerm(this, m_mailScrollRect.content.GetChild(i).gameObject));
        }

        for (int i = 0; i < m_attachmentScrollRect.content.childCount; i++) {
          m_attachmentTermList.Add(new UIShortItemInfo(m_attachmentScrollRect.content.GetChild(i).gameObject));
        }

        m_mailScrollView = new UIScrollView<MailDB, UIMailTerm>(m_mailScrollRect, m_mailGLG, m_mails, m_mailTermList);
        m_attachmentScrollView = new UIScrollView<ItemDB, UIShortItemInfo>(m_attachmentScrollRect, m_attachmentGLG,
                                                                           new List<ItemDB>(), m_attachmentTermList);

        _CheckGetAllButton();
        _AddListener();

        EventCenter.AddListener(Broadcast.EventType.REFRESH_MAIL_VIEW, _RefreshMailView);
      }

      private void _AddListener() {
        m_getButton.onClick.AddListener(_GetAttachment);
        m_getAllButton.onClick.AddListener(_GetAllAttachment);
        m_deleteAllButton.onClick.AddListener(_DeleteAllMails);
      }

      public void SwitchMail(MailDB mail) {
        if (mail != null) {
          m_mailContentPanel.SetActiveEx(true);
          m_signatureText.text = mail.signature;
          m_dateText.text = Convert.ToDateTime(mail.dateTime).ToShortDateString();
          m_contentText.text = string.Format(mail.content, DBManager.instance.userName);

          m_selectedMail = mail;

          _CheckGetButton();

          m_attachmentScrollView.OnRefresh(new List<ItemDB>(mail.attachment));
        }
      }

      public void Destroy() {
        foreach (var mailTerm in m_mailTermList) {
          mailTerm.Destroy();
        }
        EventCenter.RemoveListener(Broadcast.EventType.REFRESH_MAIL_VIEW, _RefreshMailView);
      }

      private void _CheckGetButton() {
        if (m_selectedMail != null) {
          m_getButton.interactable = !m_selectedMail.isReceived;
          m_getButton.textComponent.text = m_selectedMail.isReceived ? StringMap.GOT : StringMap.CLICK_GET;
          m_getButton.image.color = m_selectedMail.isReceived ? Colors.DISABLE_BUTTON : m_getButton.colors.normalColor;
        }
      }

      private void _CheckGetAllButton() {
        foreach (MailDB mail in m_mails) {
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
        if (m_selectedMail != null && !m_selectedMail.isReceived) {
          m_selectedMail.GetAttachment();

          _CheckGetAllButton();
          _CheckGetButton();

          DBManager.instance.UpdateUserData();
        }
      }

      private void _GetAllAttachment() {
        var tempMailList = new List<MailDB>();

        foreach (var mail in m_mails) {
          if (!mail.isReceived) {
            tempMailList.Add(mail);
          }
        }

        foreach (var mail in tempMailList) {
          mail.GetAttachment();
          if (mail == m_selectedMail) {
            _CheckGetButton();
          }
        }
        m_getAllButton.interactable = false;
        m_getAllButton.image.color = Colors.DISABLE_BUTTON;

        DBManager.instance.UpdateUserData();
      }

      private void _DeleteAllMails() {
        List<MailDB> tempMailList = new List<MailDB>();

        for (int i = m_mails.Count-1; i >=0; i--) {
          if (m_mails[i].isReceived) {
            tempMailList.Add(m_mails[i]);
          }
        }

        if (m_selectedMail != null) {
          //这种选中某个子项UI的记得在子项UI被释放时也一并释放掉
          //其他的还有UIBag的UIItemInfoBoard
          m_selectedMail = null;
          m_mailContentPanel.SetActiveEx(false);
        }

        DBManager.instance.DeleteMails(tempMailList.ToArray());
        DBManager.instance.UpdateUserData();
      }

      //当有新邮件时会触发
      private void _RefreshMailView() {
        _CheckGetAllButton();
        m_mailScrollView.OnRefresh();
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

    private void _AddListener() {
      GetComponent<Button>("close").onClick.AddListener(() => UIManager.instance.CloseOne(name));
    }

    public override void Destroy() {
      base.Destroy();
      m_mailGroup.Destroy();
    }
  }
}