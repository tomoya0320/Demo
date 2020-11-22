using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Yume.Broadcast;
using Yume.Battle;
using Yume.DB;
using Yume.MailSystem;

namespace Yume.UI {
  public class MailViewController : MonoBehaviour {
    [SerializeField]
    private Sprite _selectedImg;
    [SerializeField]
    private Sprite _unselectedImg;
    [SerializeField]
    private GameObject _noMailPanel;
    [SerializeField]
    private GameObject _mailContentPanel;
    [SerializeField]
    private RectTransform _mailListContent;
    [SerializeField]
    private RectTransform _attachmentContent;
    [SerializeField]
    private Text _contentText;
    [SerializeField]
    private Text _signatureText;
    [SerializeField]
    private Text _dateText;
    [SerializeField]
    private Button _getButton;
    [SerializeField]
    private Button _getAllButton;
    [SerializeField]
    private GameObject _mailViewPrefab;
    [SerializeField]
    private GameObject _attachmentPrefab;

    private List<MailDB> m_mails;
    private MailView m_currentClickedMail;
    private GridLayoutGroup m_mailGLG;
    private GridLayoutGroup m_attachmentGLG;

    private void Start() {
      EventCenter.AddListener(Broadcast.EventType.ADD_MAIL_VIEW, _AddMailView);

      m_mails = DBManager.userMails;
      _noMailPanel.SetActive(m_mails.Count == 0);

      foreach (MailDB mail in m_mails) {
        GameObject tempMail = Instantiate(_mailViewPrefab, _mailListContent);
        tempMail.GetComponent<MailView>().OnInit(mail, this);
      }

      m_mailGLG = _mailListContent.GetComponent<GridLayoutGroup>();
      m_attachmentGLG = _attachmentContent.GetComponent<GridLayoutGroup>();

      _AdjustMailListContentHeight();
      _CheckGetAllButton();
    }

    private void _AdjustMailListContentHeight() {
      float height = m_mails.Count * (m_mailGLG.spacing.y + m_mailGLG.cellSize.y);
      _mailListContent.sizeDelta = new Vector2(_mailListContent.sizeDelta.x, height);
    }

    public void SwitchMail(MailView mailView) {
      if (m_currentClickedMail != null) {
        m_currentClickedMail.bottomImg.sprite = _unselectedImg;
      }

      _mailContentPanel.SetActive(true);
      mailView.bottomImg.sprite = _selectedImg;
      _contentText.text = string.Format(mailView.mail.content, DBManager.userName);
      _signatureText.text = mailView.mail.signature;
      _dateText.text = Convert.ToDateTime(mailView.mail.dateTime).ToShortDateString();

      for (int i = 0; i < _attachmentContent.childCount; i++) {
        Destroy(_attachmentContent.GetChild(i).gameObject);
      }

      float width = mailView.mail.attachment.Length * (m_attachmentGLG.spacing.x + m_attachmentGLG.cellSize.x);
      _attachmentContent.sizeDelta = new Vector2(width, _attachmentContent.sizeDelta.y);

      foreach (ItemDB item in mailView.mail.attachment) {
        GameObject tempItem = Instantiate(_attachmentPrefab, _attachmentContent);
        Color iconColor = mailView.mail.isReceived ? Color.white * 0.8f : Color.white;
        tempItem.GetComponent<ShortItemInfoView>().OnInit(item, iconColor);
      }

      m_currentClickedMail = mailView;
      _getButton.interactable = !mailView.mail.isReceived;
    }

    private void OnDestroy() {
      EventCenter.RemoveListener(Broadcast.EventType.ADD_MAIL_VIEW, _AddMailView);
    }

    private void _CheckGetAllButton() {
      _getAllButton.interactable = false;
      foreach (MailDB mail in m_mails) {
        _getAllButton.interactable |= !mail.isReceived;
      }
    }

    public void GetAttachment() {
      if (m_currentClickedMail && !m_currentClickedMail.mail.isReceived) {
        m_currentClickedMail.mail.GetAttachment();
        foreach (ShortItemInfoView siif in _attachmentContent.GetComponentsInChildren<ShortItemInfoView>()) {
          siif.iconImg.color = Color.white * 0.8f;
        }
        _getButton.interactable = false;
        _CheckGetAllButton();
      }
    }

    public void GetAllAttachment() {
      GetAttachment();
      foreach (MailView mailInList in _mailListContent.GetComponentsInChildren<MailView>()) {
        if (!mailInList.mail.isReceived) {
          mailInList.mail.GetAttachment();
        }
      }
      _getAllButton.interactable = false;
    }

    public void DeleteAllMails() {
      foreach (MailView mailInList in _mailListContent.GetComponentsInChildren<MailView>()) {
        if (mailInList.mail.isReceived) {
          mailInList.mail.Delete();
          if (mailInList == m_currentClickedMail) {
            _mailContentPanel.SetActive(false);
          }
          Destroy(mailInList.gameObject);
        }
      }
      _noMailPanel.SetActive(m_mails.Count == 0);
      _AdjustMailListContentHeight();
    }

    private void _AddMailView() {
      _noMailPanel.SetActive(false);
      GameObject tempMail = Instantiate(_mailViewPrefab, _mailListContent);
      MailDB mail = m_mails[m_mails.Count - 1];
      tempMail.GetComponent<MailView>().OnInit(mail, this);
      _getAllButton.interactable |= !mail.isReceived;
      _AdjustMailListContentHeight();
    }
  }
}