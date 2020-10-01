using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using Yume.Broadcast;
using Yume.DB;
using Yume.String;
using Yume.UI;

namespace Yume.MailSystem {
  public class MailShowController : MonoBehaviour {
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

    private List<MailDB> mails;
    private MailShow currentClickedMail;
    private GameObject mailInListPrefab;
    private GameObject attachmentPrefab;
    private GridLayoutGroup mailGLG;
    private GridLayoutGroup attachmentGLG;

    private async void Start() {
      EventCenter.AddListener(Broadcast.EventType.ADD_MAIL, _AddMail);

      mailInListPrefab = await Addressables.LoadAssetAsync<GameObject>(StringMap.MAIL_SHOW).Task;
      attachmentPrefab = await Addressables.LoadAssetAsync<GameObject>(StringMap.ATTACHMENT_PREFAB).Task;
      
      mails = DBManager.userMails;
      _noMailPanel.SetActive(mails.Count == 0);

      foreach (MailDB mail in mails) {
        GameObject tempMail = Instantiate(mailInListPrefab, _mailListContent);
        tempMail.GetComponent<MailShow>().OnInit(mail, this);
      }

      mailGLG = _mailListContent.GetComponent<GridLayoutGroup>();
      attachmentGLG = _attachmentContent.GetComponent<GridLayoutGroup>();

      _AdjustMailListContentHeight();
      _CheckGetAllButton();
    }

    private void _AdjustMailListContentHeight() {
      float height = mails.Count * (mailGLG.spacing.y + mailGLG.cellSize.y);
      _mailListContent.sizeDelta = new Vector2(_mailListContent.sizeDelta.x, height);
    }

    public void SwitchMail(MailShow mailInList) {
      if (currentClickedMail != null) {
        currentClickedMail.GetComponent<Image>().sprite = _unselectedImg;
      }

      _mailContentPanel.SetActive(true);
      mailInList.GetComponent<Image>().sprite = _selectedImg;
      _contentText.text = string.Format(mailInList.mail.content.Replace("[space]", " ")/*http的header里不能有空格所以要替换一下，下同*/, DBManager.userName);
      _signatureText.text = mailInList.mail.signature.Replace("[space]", " ");
      _dateText.text = Convert.ToDateTime(mailInList.mail.dateTime).ToShortDateString();

      for (int i = 0; i < _attachmentContent.childCount; i++) {
        Destroy(_attachmentContent.GetChild(i).gameObject);
      }

      float width = mailInList.mail.attachment.Length * (attachmentGLG.spacing.x + attachmentGLG.cellSize.x);
      _attachmentContent.sizeDelta = new Vector2(width, _attachmentContent.sizeDelta.y);

      foreach (ItemDB item in mailInList.mail.attachment) {
        GameObject tempItem = Instantiate(attachmentPrefab, _attachmentContent);
        Color iconColor = mailInList.mail.isReceived ? Color.white * 0.8f : Color.white;
        tempItem.GetComponent<ShortItemInfoShow>().OnInit(item, iconColor);
      }

      currentClickedMail = mailInList;
      _getButton.interactable = !mailInList.mail.isReceived;
    }

    private void OnDestroy() {
      EventCenter.RemoveListener(Broadcast.EventType.ADD_MAIL, _AddMail);
    }

    private void _CheckGetAllButton() {
      _getAllButton.interactable = false;
      foreach (MailDB mail in mails) {
        _getAllButton.interactable |= !mail.isReceived;
      }
    }

    public void GetAttachment() {
      if (currentClickedMail && !currentClickedMail.mail.isReceived) {
        currentClickedMail.mail.GetAttachment();
        foreach (ShortItemInfoShow siif in _attachmentContent.GetComponentsInChildren<ShortItemInfoShow>()) {
          siif.iconImg.color = Color.white * 0.8f;
        }
        _getButton.interactable = false;
        _CheckGetAllButton();
      }
    }

    public void GetAllAttachment() {
      GetAttachment();
      foreach (MailShow mailInList in _mailListContent.GetComponentsInChildren<MailShow>()) {
        if (!mailInList.mail.isReceived) {
          mailInList.mail.GetAttachment();
        }
      }
      _getAllButton.interactable = false;
    }

    public void DeleteAllMails() {
      foreach (MailShow mailInList in _mailListContent.GetComponentsInChildren<MailShow>()) {
        if (mailInList.mail.isReceived) {
          mailInList.mail.Delete();
          if (mailInList == currentClickedMail) {
            _mailContentPanel.SetActive(false);
          }
          Destroy(mailInList.gameObject);
        }
      }
      _noMailPanel.SetActive(mails.Count == 0);
      _AdjustMailListContentHeight();
    }

    private void _AddMail() {
      _noMailPanel.SetActive(false);
      GameObject tempMail = Instantiate(mailInListPrefab, _mailListContent);
      MailDB mail = mails[mails.Count - 1];
      tempMail.GetComponent<MailShow>().OnInit(mail, this);
      _getAllButton.interactable |= !mail.isReceived;
      _AdjustMailListContentHeight();
    }
  }
}