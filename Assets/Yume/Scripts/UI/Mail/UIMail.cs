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
    private class UIMailTerm {
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

      public UIMailTerm(MailDB mail, UIMailGroup mailController, GameObject root) {
        m_root = root;
        var uiObject = root.GetComponent<UIObject>();
        m_titleText = uiObject.GetComponent<Text>("TitleText");
        m_bottomImg = uiObject.GetComponent<Image>("BottomImage");
        m_clickButton = uiObject.GetComponent<Button>("ClickButton");
        m_dateDeltaText = uiObject.GetComponent<Text>("DateDeltaText");
        m_attachmentImg = uiObject.GetComponent<Image>("AttachmentImage");

        m_mail = mail;
        m_titleText.text = mail.title;
        m_mailController = mailController;
        m_attachmentImg.enabled = mail.attachment.Length > 0;
        m_dateDeltaText.text = MathUtil.CalDateDeltaFromNow(mail.dateTime);

        _AddListener();
      }

      private void _AddListener() {
        m_clickButton.onClick.AddListener(() => m_mailController.SwitchMail(this));
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
      private GameObject m_mailTermPrefab;
      private GameObject m_attachmentPrefab;

      private List<MailDB> m_mails;
      private UIMailTerm m_currentClickedMail;
      private GridLayoutGroup m_mailGLG;
      private GridLayoutGroup m_attachmentGLG;

      private List<UIMailTerm> m_mailTermList = new List<UIMailTerm>();

      public UIMailGroup(GameObject root) {
        var uiObject = root.GetComponent<UIObject>();
        m_selectedImg = uiObject.GetSprite("SelectedImage");
        m_unselectedImg = uiObject.GetSprite("UnselectedImage");
        m_dateText = uiObject.GetComponent<Text>("DateText");
        m_noMailPanel = uiObject.GetGameObject("NoMailPanel");
        m_getButton = uiObject.GetComponent<Button>("GetButton");
        m_contentText = uiObject.GetComponent<Text>("ContentText");
        m_mailTermPrefab = uiObject.GetGameObject("MailTermPrefab");
        m_signatureText = uiObject.GetComponent<Text>("SignatureText");
        m_getAllButton = uiObject.GetComponent<Button>("GetAllButton");
        m_mailContentPanel = uiObject.GetGameObject("MailContentPanel");
        m_attachmentPrefab = uiObject.GetGameObject("AttachmentPrefab");
        m_deleteAllButton = uiObject.GetComponent<Button>("DeleteAllButton");
        m_mailListContent = uiObject.GetGameObject("MailListContent").GetRect();
        m_attachmentContent = uiObject.GetGameObject("AttachmentContent").GetRect();

        m_mails = DBManager.instance.userMails;
        m_noMailPanel.SetActive(m_mails.Count == 0);

        foreach (MailDB mail in m_mails) {
          GameObject tempGo = UnityEngine.Object.Instantiate(m_mailTermPrefab, m_mailListContent);
          m_mailTermList.Add(new UIMailTerm(mail, this, tempGo));
        }

        m_mailGLG = m_mailListContent.GetComponent<GridLayoutGroup>();
        m_attachmentGLG = m_attachmentContent.GetComponent<GridLayoutGroup>();

        _AdjustMailListContentHeight();
        _CheckGetAllButton();
        _AddListener();

        EventCenter.AddListener(Broadcast.EventType.ADD_MAIL_TERM, _AddMailView);
      }

      private void _AddListener() {
        m_getButton.onClick.AddListener(_GetAttachment);
        m_getAllButton.onClick.AddListener(_GetAllAttachment);
        m_deleteAllButton.onClick.AddListener(_DeleteAllMails);
      }

      private void _AdjustMailListContentHeight() {
        float height = Mathf.Max(0, m_mails.Count * (m_mailGLG.spacing.y + m_mailGLG.cellSize.y) - m_mailGLG.spacing.y);
        m_mailListContent.sizeDelta = new Vector2(m_mailListContent.sizeDelta.x, height);
      }

      public void SwitchMail(UIMailTerm mailView) {
        if (m_currentClickedMail != null) {
          m_currentClickedMail.bottomImg.sprite = m_unselectedImg;
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

        m_currentClickedMail = mailView;
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
        if (m_currentClickedMail != null && !m_currentClickedMail.mail.isReceived) {
          m_currentClickedMail.mail.GetAttachment();
          _CurMailInfoDisable();
          _CheckGetAllButton();

          DBManager.instance.UpdateUserData();
        }
      }

      private void _GetAllAttachment() {
        var tempMailList = new List<MailDB>();
        foreach (var mailTerm in m_mailTermList) {
          if (!mailTerm.mail.isReceived) {
            tempMailList.Add(mailTerm.mail);
          }
        }
        if (tempMailList.Count > 0) {
          foreach (var mail in tempMailList) {
            mail.GetAttachment();
            if (mail == m_currentClickedMail?.mail) {
              _CurMailInfoDisable();
            }
          }
          m_getAllButton.interactable = false;

          DBManager.instance.UpdateUserData();
        }
      }

      private void _DeleteAllMails() {
        for (int i = m_mailTermList.Count - 1; i >= 0; i--) {
          if (m_mailTermList[i].mail.isReceived) {
            m_mailTermList[i].mail.Delete();
            if (m_mailTermList[i] == m_currentClickedMail) {
              m_mailContentPanel.SetActive(false);
            }
            if (m_currentClickedMail == m_mailTermList[i]) {
              m_currentClickedMail = null;
            }
            UnityEngine.Object.Destroy(m_mailTermList[i].gameObject);
            m_mailTermList.RemoveAt(i);
          }
        }
        m_noMailPanel.SetActive(m_mails.Count == 0);
        _AdjustMailListContentHeight();

        DBManager.instance.UpdateUserData();
      }

      private void _CurMailInfoDisable() {
        foreach (ShortItemInfoView siif in m_attachmentContent.GetComponentsInChildren<ShortItemInfoView>()) {
          siif.iconImg.color = Color.white * 0.8f;
        }
        m_getButton.interactable = false;
      }

      private void _AddMailView() {
        m_noMailPanel.SetActive(false);
        GameObject tempGo = UnityEngine.Object.Instantiate(m_mailTermPrefab, m_mailListContent);
        MailDB mail = m_mails[m_mails.Count - 1];
        m_mailTermList.Add(new UIMailTerm(mail, this, tempGo));
        m_getAllButton.interactable |= !mail.isReceived;
        _AdjustMailListContentHeight();
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