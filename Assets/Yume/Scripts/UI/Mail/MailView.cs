using UnityEngine;
using UnityEngine.UI;
using Yume.Math;

namespace Yume.MailSystem {
  public class MailView : MonoBehaviour {
    [SerializeField]
    private Image _attachmentImg;
    [SerializeField]
    private Text _titleText;
    [SerializeField]
    private Text _dateDeltaText;
    [SerializeField]
    private Image _bottomImg;
    public Image bottomImg => _bottomImg;
    public MailDB mail { private set; get; }
    private MailViewController m_mailController;

    public void OnInit(MailDB mail, MailViewController mailController) {
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