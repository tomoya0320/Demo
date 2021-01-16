using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Yume.UI {
  public class YTextAutoScroll : MonoBehaviour {
    [SerializeField]
    private float _scrollSpeed = 10.0f;
    [SerializeField]
    private float _delayTime = 3.0f;
    private Tween m_tween;
    private Text m_textComponent;
    private RectTransform m_rectTransform;
    private RectTransform m_textRectTransform;

    private void Awake() {
      m_textComponent = GetComponentInChildren<Text>(true);
      m_textRectTransform = m_textComponent.rectTransform;
      m_rectTransform = GetComponent<RectTransform>();
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
        .DOAnchorPosY(endValue, _scrollSpeed)
        .SetSpeedBased()
        .SetDelay(_delayTime)
        .OnComplete(() => {
          m_tween = m_textComponent.DOFade(0, 0.5f)
          .SetDelay(_delayTime)
          .OnComplete(() => {
            m_textRectTransform.anchoredPosition = Vector2.zero;
            m_tween = m_textComponent.DOFade(1.0f, 0.5f)
            .OnComplete(() => _ScrollTextInternal(endValue));
          });
        });
    }
  }
}