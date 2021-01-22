using DG.Tweening;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UI;
using Yume.UI;
using Yume.Resource;

namespace Yume.AVG {
  public class AVGController : SingletonMono<AVGController> {
    [SerializeField]
    private CanvasGroup _canvasGroup;
    [SerializeField]
    private Image _bgImage;
    [SerializeField]
    private Text _nameText;
    [SerializeField]
    private Text _dialogueText;
    private Tween m_textTween;
    private JArray m_currentTexts;
    private JObject m_textJObject;
    private int m_currentTextIndex;

    public void StartAVG(string avgName) {
      var avgJson = ResourceManager.GetAvgJson(avgName);
      if (avgJson != null) {
        m_currentTexts = avgJson;
        m_currentTextIndex = -1;
        _MoveToNext();
      } else {
        _QuitAVGScene();
      }
    }

    public void EventOnClick() {
      if (m_textTween == null) {
        _MoveToNext();
      } else {
        m_textTween.Complete();
      }
    }

    private void _MoveToNext() {
      m_currentTextIndex++;
      if (m_currentTextIndex < m_currentTexts.Count) {
        m_textJObject = m_currentTexts[m_currentTextIndex].Value<JObject>();
        if (m_textJObject.ContainsKey(StringMap.BG)) {
          _bgImage.sprite = ResourceManager.LoadAsset<Sprite>(
                            m_textJObject[StringMap.BG].Value<string>());
        }
        _nameText.text = m_textJObject[StringMap.NAME].Value<string>();
        _dialogueText.text = StringMap.EMPTY;
        m_textTween = _dialogueText.DOText(m_textJObject[StringMap.DIALOGUE].Value<string>(), 10.0f)
                      .SetEase(Ease.Linear).SetSpeedBased().OnComplete(() => m_textTween = null);
      } else {
        _QuitAVGScene();
      }
    }

    private void _QuitAVGScene() {
      UIManager.OpenOne("blackMask", () => {
        Destroy(gameObject);
        UIManager.CloseOne("blackMask");
      });
    }
  }
}