using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Yume.Broadcast;
using Yume.Battle;
using Yume.DB;
using Yume.MailSystem;
using Yume.Math;

namespace Yume.UI {
  public class UIMail : UIBase {
    private class UIMailTerm : IScrollTerm<MailDB> {
      private MailDB m_mail;
      private Text m_titleText;
      private Image m_bottomImg;
      private GameObject m_root;
      private Button m_clickButton;
      private Text m_dateDeltaText;
      private Image m_attachmentImg;
      private UIMailGroup m_mailController;

      public MailDB mail => m_mail;
      public Image bottomImg => m_bottomImg;
      public GameObject gameObject => m_root;

      public UIMailTerm(UIMailGroup mailController, GameObject root) {
        m_root = root;
        m_mailController = mailController;

        var uiObject = m_root.GetComponent<UIObject>();
        m_titleText = uiObject.GetComponent<Text>("TitleText");
        m_bottomImg = uiObject.GetComponent<Image>("BottomImage");
        m_clickButton = uiObject.GetComponent<Button>("ClickButton");
        m_dateDeltaText = uiObject.GetComponent<Text>("DateDeltaText");
        m_attachmentImg = uiObject.GetComponent<Image>("AttachmentImage");

        _AddListener();
      }

      private void _AddListener() {
        m_clickButton.onClick.AddListener(() => m_mailController.SwitchMail(this));
      }

      public void Update(MailDB data) {
        m_mail = data;
        if (m_mail == null) {
          gameObject.SetActive(false);
        } else {
          gameObject.SetActive(true);
          m_titleText.text = mail.title;
          m_attachmentImg.enabled = mail.attachment.Length > 0;
          m_dateDeltaText.text = MathUtil.CalDateDeltaFromNow(mail.dateTime);
        }
      }
    }

    private class UIMailGroup {
      private Sprite m_selectedImg;
      private Sprite m_unselectedImg;
      private Text m_contentText;
      private Text m_signatureText;
      private Text m_dateText;
      private Button m_getButton;
      private Button m_getAllButton;
      private Button m_deleteAllButton;
      private RectTransform m_mailListContent;
      private RectTransform m_attachmentContent;
      private GameObject m_noMailPanel;
      private GameObject m_mailContentPanel;
      private GameObject m_attachmentPrefab;

      private List<MailDB> m_mails;
      private GridLayoutGroup m_mailGLG;
      private ScrollRect m_mailScrollRect;
      private UIMailTerm m_curClickedMailTerm;
      private GridLayoutGroup m_attachmentGLG;
      private UIScrollView<MailDB, UIMailTerm> m_mailScrollView;
      private List<UIMailTerm> m_mailTermList = new List<UIMailTerm>();

      public UIMailGroup(GameObject root) {
        var uiObject = root.GetComponent<UIObject>();
        m_selectedImg = uiObject.GetSprite("SelectedImage");
        m_dateText = uiObject.GetComponent<Text>("DateText");
        m_noMailPanel = uiObject.GetGameObject("NoMailPanel");
        m_unselectedImg = uiObject.GetSprite("UnselectedImage");
        m_getButton = uiObject.GetComponent<Button>("GetButton");
        m_contentText = uiObject.GetComponent<Text>("ContentText");
        m_signatureText = uiObject.GetComponent<Text>("SignatureText");
        m_getAllButton = uiObject.GetComponent<Button>("GetAllButton");
        m_mailContentPanel = uiObject.GetGameObject("MailContentPanel");
        m_attachmentPrefab = uiObject.GetGameObject("AttachmentPrefab");
        m_deleteAllButton = uiObject.GetComponent<Button>("DeleteAllButton");
        m_mailScrollRect = uiObject.GetComponent<ScrollRect>("MailScrollRect");
        m_mailListContent = uiObject.GetGameObject("MailListContent").GetRect();
        m_attachmentContent = uiObject.GetGameObject("AttachmentContent").GetRect();

        m_mails = DBManager.instance.userMails;
        m_noMailPanel.SetActive(m_mails.Count == 0);

        m_mailGLG = m_mailListContent.GetComponent<GridLayoutGroup>();
        m_attachmentGLG = m_attachmentContent.GetComponent<GridLayoutGroup>();
       
        for (int i = 0; i < m_mailListContent.childCount; i++) {
          m_mailTermList.Add(new UIMailTerm(this, m_mailListContent.GetChild(i).gameObject));
        }

        m_mailScrollView = new UIScrollView<MailDB, UIMailTerm>(m_mailScrollRect, m_mailGLG, m_mails, m_mailTermList);

        _CheckGetAllButton();
        _AddListener();

        EventCenter.AddListener(Broadcast.EventType.ADD_MAIL_TERM, _AddMailView);
      }

      private void _AddListener() {
        m_getButton.onClick.AddListener(_GetAttachment);
        m_getAllButton.onClick.AddListener(_GetAllAttachment);
        m_deleteAllButton.onClick.AddListener(_DeleteAllMails);
      }

      public void SwitchMail(UIMailTerm mailView) {
        if (m_curClickedMailTerm != null) {
          m_curClickedMailTerm.bottomImg.sprite = m_unselectedImg;
        }

        m_mailContentPanel.SetActive(true);
        mailView.bottomImg.sprite = m_selectedImg;
        m_contentText.text = string.Format(mailView.mail.content, DBManager.instance.userName);
        m_signatureText.text = mailView.mail.signature;
        m_dateText.text = Convert.ToDateTime(mailView.mail.dateTime).ToShortDateString();

        Color iconColor = mailView.mail.isReceived ? Color.white * 0.8f : Color.white;
        if (mailView.mail.attachment.Length > m_attachmentContent.childCount) {
          for (int i = 0; i < mailView.mail.attachment.Length; i++) {
            if (i < m_attachmentContent.childCount) {
              _OnInitAttachment(m_attachmentContent.GetChild(i), mailView.mail.attachment[i], iconColor);
            } else {
              var tempAttachment = UnityEngine.Object.Instantiate(m_attachmentPrefab, m_attachmentContent);
              tempAttachment.GetComponent<ShortItemInfoView>().OnInit(mailView.mail.attachment[i], iconColor);
            }
          }
        } else {
          for (int i = 0; i < m_attachmentContent.childCount; i++) {
            if (i < mailView.mail.attachment.Length) {
              _OnInitAttachment(m_attachmentContent.GetChild(i), mailView.mail.attachment[i], iconColor);
            } else {
              _RecycleAttachment(m_attachmentContent.GetChild(i));
            }
          }
        }

        float width = mailView.mail.attachment.Length * (m_attachmentGLG.spacing.x + m_attachmentGLG.cellSize.x);
        m_attachmentContent.sizeDelta = new Vector2(width - m_attachmentGLG.spacing.x, m_attachmentContent.sizeDelta.y);

        m_curClickedMailTerm = mailView;
        m_getButton.interactable = !mailView.mail.isReceived;
      }

      private void _RecycleAttachment(Transform attachment) {
        if (attachment.gameObject.activeSelf) {
          attachment.gameObject.SetActive(false);
        }
      }

      private void _OnInitAttachment(Transform attachment, ItemDB item, Color iconColor) {
        if (!attachment.gameObject.activeSelf) {
          attachment.gameObject.SetActive(true);
        }
        attachment.GetComponent<ShortItemInfoView>().OnInit(item, iconColor);
      }

      public void Destroy() {
        EventCenter.RemoveListener(Broadcast.EventType.ADD_MAIL_TERM, _AddMailView);
      }

      private void _CheckGetAllButton() {
        m_getAllButton.interactable = false;
        foreach (MailDB mail in m_mails) {
          m_getAllButton.interactable |= !mail.isReceived;
        }
      }

      private void _GetAttachment() {
        if (m_curClickedMailTerm != null && !m_curClickedMailTerm.mail.isReceived) {
          m_curClickedMailTerm.mail.GetAttachment();
          _CurMailInfoDisable();
          _CheckGetAllButton();

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
        if (tempMailList.Count > 0) {
          foreach (var mail in tempMailList) {
            mail.GetAttachment();
            if (mail == m_curClickedMailTerm?.mail) {
              _CurMailInfoDisable();
            }
          }
          m_getAllButton.interactable = false;

          DBManager.instance.UpdateUserData();
        }
      }

      private void _DeleteAllMails() {
        for (int i = m_mails.Count-1; i >=0; i--) {
          if (m_mails[i].isReceived) {
            m_mails[i].Delete();
          }
        }

        m_mailScrollView.OnRefresh();
        m_noMailPanel.SetActive(m_mails.Count == 0);

        DBManager.instance.UpdateUserData();
      }

      private void _CurMailInfoDisable() {
        foreach (ShortItemInfoView siif in m_attachmentContent.GetComponentsInChildren<ShortItemInfoView>()) {
          siif.iconImg.color = Color.white * 0.8f;
        }
        m_getButton.interactable = false;
      }

      private void _AddMailView() {
        m_mailScrollView.OnRefresh();
        m_noMailPanel.SetActive(false);
        m_getAllButton.interactable |= !m_mails[m_mails.Count - 1].isReceived;
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