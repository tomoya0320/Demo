using UnityEngine;
using UnityEngine.UI;
using Yume.Math;

namespace Yume.MailSystem {
  public class MailShow : MonoBehaviour {
    [SerializeField]
    private Image _attachmentImg;
    [SerializeField]
    private Text _titleText;
    [SerializeField]
    private Text _dateDeltaText;
    public Mail mail { private set; get; }
    private MailShowController m_mailController;

    public void OnInit(Mail mail, MailShowController mailController) {
      this.mail = mail;
      m_mailController = mailController;
      _attachmentImg.enabled = mail.attachment.Length > 0;
      _titleText.text = mail.title;
      _dateDeltaText.text = MathUtil.CalDateDeltaFromNow(mail.dateTime);
    }

    public void OnClick() {
      m_mailController.SwitchMail(this);
    }
  }
}