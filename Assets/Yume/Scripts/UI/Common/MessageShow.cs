using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace Yume.UI {
  public class MessageShow : MonoBehaviour {
    [SerializeField]
    private Text _description;
    [SerializeField]
    private Image _image;
    private Message m_message;
    private RectTransform m_textRect;

    public void OnInit(Message message) {
      m_message = message;
      _description.text = message.description;
      m_textRect = _description.GetComponent<RectTransform>();
      Vector2 size = m_textRect.rect.size;
      // height保持不变
      m_textRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
      // 动态设置width
      m_textRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,
         Mathf.Clamp(_description.preferredWidth, 300, 1500));

      if (message.isShowImg) {
        if (message.img != null) {
          _image.sprite = message.img;
        }
      } else {
        _image.enabled = false;
      }
      switch (message.style) {
        case Message.Style.SLIDE: {
            RectTransform rectTransform = GetComponent<RectTransform>();
            rectTransform.DOAnchorPosY(-m_textRect.rect.height, message.duration).SetRelative().OnComplete(() =>
                rectTransform.DOAnchorPosY(m_textRect.rect.height, message.duration).
                SetDelay(message.delay).SetRelative().OnComplete(_ShowNextMessage)
            );
            break;
          }
        case Message.Style.FADE: {
            CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
            canvasGroup.DOFade(1.0f, message.duration).OnComplete(() =>
                canvasGroup.DOFade(0, message.duration).SetDelay(message.delay).OnComplete(_ShowNextMessage)
            );
            break;
          }
      }
    }

    private void _ShowNextMessage() {
      UIController.instance.messageManager.RemoveMessage(m_message);
      UIController.instance.messageManager.ShowMessage();
      Destroy(gameObject);
    }
  }
}