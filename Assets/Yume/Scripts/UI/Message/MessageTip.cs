using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace Yume.UI {
  public abstract class MessageTip : UIBase {
    protected Image image;
    protected Message message;
    protected Text description;
    protected RectTransform rect;
    protected RectTransform textRect;

    public override void OnInit(UIData uiData, GameObject root) {
      base.OnInit(uiData, root);
      image = GetComponent<Image>("image");
      description = GetComponent<Text>("description");
      rect = root.GetComponent<RectTransform>();
      textRect = description.GetComponent<RectTransform>();
    }

    public override void Open(params object[] args) {
      message = (Message)args[0];

      if (message.isShowImg) {
        if (message.img != null) {
          image.sprite = message.img;
        }
      } else {
        image.enabled = false;
      }

      description.text = message.description;

      Vector2 size = textRect.rect.size;
      // height保持不变
      textRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
      // 动态设置width
      textRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,
         Mathf.Clamp(description.preferredWidth, 300, 1500));

      PlayOpenAnim(args);
    }

    public override void Close(params object[] args) {
      //因为使用了DOTween，而且在DOTween动画结束时提示框相当于隐藏了所以不需要使gameObject disable（隐藏了反而会影响动画）
    }

    protected abstract void PlayOpenAnim(params object[] args);

    protected void ShowNextMessage(params object[] args) {
      Close(args);
      MessageManager.instance.RemoveMessage(message);
      MessageManager.instance.ShowMessage();
    }
  }

  public class SlideMessage : MessageTip {

    protected override void PlayOpenAnim(params object[] args) {
      rect.DOAnchorPosY(0, message.duration).OnComplete(() => {
        rect.DOAnchorPosY(rect.rect.height, message.duration).
        SetDelay(message.delay).OnComplete(()=> {
          UIManager.instance.CloseOne("message_slide");
          ShowNextMessage(args);
        });
      });
    }
  }

  public class FadeMessage : MessageTip {
    protected CanvasGroup canvasGroup;

    public override void OnInit(UIData uiData, GameObject root) {
      base.OnInit(uiData, root);
      canvasGroup = root.GetComponent<CanvasGroup>();
    }

    protected override void PlayOpenAnim(params object[] args) {
      canvasGroup.DOFade(1.0f, message.duration).OnComplete(() => {
        canvasGroup.DOFade(0, message.duration).SetDelay(message.delay).OnComplete(()=> {
          UIManager.instance.CloseOne("message_fade");
          ShowNextMessage(args);
        });
      });
    }
  }
}