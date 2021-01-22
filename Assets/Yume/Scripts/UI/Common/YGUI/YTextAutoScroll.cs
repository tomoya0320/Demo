using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Yume.UI {
  public class YTextAutoScroll {
    private Tween m_tween;
    private Text m_textComponent;
    private float m_delayTime = 3.0f;
    private float m_scrollSpeed = 10.0f;
    private RectTransform m_rectTransform;
    private RectTransform m_textRectTransform;

    public YTextAutoScroll(GameObject root) {
      ObjectContainer uiObject = root.GetComponent<ObjectContainer>();
      m_rectTransform = root.GetComponent<RectTransform>();
      m_textComponent = uiObject.GetComponent<Text>("Text");
      m_textRectTransform = m_textComponent.rectTransform;
    }

    public void SetTextContent(string content) {
      m_tween?.Kill();
      m_textComponent.text = content;
      LayoutRebuilder.ForceRebuildLayoutImmediate(m_textRectTransform);
      float endValue = m_textRectTransform.rect.height - m_rectTransform.rect.height;
      if (endValue > 0) {
        _ScrollTextInternal(endValue);
      }
    }

    private void _ScrollTextInternal(float endValue) {
      m_textComponent.SetAlpha(1.0f);
      m_textRectTransform.anchoredPosition = Vector2.zero;
      m_tween = m_textRectTransform
        .DOAnchorPosY(endValue, m_scrollSpeed)
        .SetSpeedBased()
        .SetDelay(m_delayTime)
        .OnComplete(() => {
          m_tween = m_textComponent.DOFade(0, 0.5f)
          .SetDelay(m_delayTime)
          .OnComplete(() => {
            m_textRectTransform.anchoredPosition = Vector2.zero;
            m_tween = m_textComponent.DOFade(1.0f, 0.5f)
            .OnComplete(() => _ScrollTextInternal(endValue));
          });
        });
    }
  }
}