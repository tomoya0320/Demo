using UnityEngine;
using UnityEngine.UI;
using Yume.Math;

namespace Yume.MailSystem {
  public class MailInList : MonoBehaviour {
    [SerializeField]
    private Image _attachmentImg;
    [SerializeField]
    private Text _titleText;
    [SerializeField]
    private Text _dateDeltaText;
    public Mail mail { private set; get; }
    private MailShow m_context;

    public void OnInit(Mail mail, MailShow context) {
      this.mail = mail;
      m_context = context;
      _attachmentImg.enabled = mail.attachment.Length > 0;
      _titleText.text = mail.title;
      _dateDeltaText.text = MathUtil.CalDateDeltaFromNow(mail.dateTime);
    }

    public void OnClick() {
      m_context.SwitchMail(this);
    }
  }
}