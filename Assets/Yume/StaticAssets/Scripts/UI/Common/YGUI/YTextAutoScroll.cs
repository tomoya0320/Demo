using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Yume.UI {
  public class YTextAutoScroll {
    private Tween m_tween;
    private Text m_textComponent;
    private CanvasGroup m_canvasGroup;
    private RectTransform m_rectTransform;
    private RectTransform m_textRectTransform;
    private const float DELAY_TIME = 3.0f;
    private const float SCROLL_SPEED = 10.0f;
    private const float ALPHA_FADE_TIME = 0.5f;

    public YTextAutoScroll(GameObject root) {
      m_canvasGroup = root.GetComponent<CanvasGroup>();
      m_rectTransform = root.GetComponent<RectTransform>();
      m_textComponent = root.GetComponentInChildren<Text>();
      m_textRectTransform = m_textComponent.rectTransform;
    }

    public void SetTextContent(string content) {
      if (m_textComponent.text == content) {
        return;
      }
      
      if (m_tween != null) {
        m_tween.Kill();
        m_tween = null;
        // 恢复默认值
        m_canvasGroup.alpha = 1.0f;
        m_textRectTransform.anchoredPosition = Vector2.zero;
      }
      
      m_textComponent.text = content;
      m_textRectTransform.SetSizeY(m_textComponent.preferredHeight);
      float endValue = m_textRectTransform.rect.height - m_rectTransform.rect.height;
      
      if (endValue > 0) {
        _ScrollTextInternal(endValue);
      }
    }

    private void _ScrollTextInternal(float endValue) {
      m_textRectTransform.anchoredPosition = Vector2.zero;
      m_tween = m_textRectTransform
                .DOAnchorPosY(endValue, SCROLL_SPEED)
                .SetSpeedBased()
                .SetDelay(DELAY_TIME)
                .OnComplete(
                  () => {
                    m_tween = m_canvasGroup.DOFade(0, ALPHA_FADE_TIME)
                                           .SetDelay(DELAY_TIME)
                                           .OnComplete(
                                             () => {
                                               m_textRectTransform.anchoredPosition = Vector2.zero;
                                               m_tween = m_canvasGroup.DOFade(1.0f, ALPHA_FADE_TIME)
                                                                      .OnComplete(() => _ScrollTextInternal(endValue));
                                             }
                                           );
                  }
                );
    }
  }
}