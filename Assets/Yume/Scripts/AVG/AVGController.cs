using DG.Tweening;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Yume.String;
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
    private Dictionary<string, JArray> m_avgTextDict;

    protected override void OnInit() {
      dontDestroyOnLoad = false;
      base.OnInit();
      m_avgTextDict = JsonConvert.DeserializeObject<Dictionary<string, JArray>>(AssetLoader.instance.GetValue<string>("avgs"));
    }

    public void StartAVG(string avgName) {
      if (m_avgTextDict.ContainsKey(avgName)) {
        m_currentTexts = m_avgTextDict[avgName];
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

    private async void _MoveToNext() {
      m_currentTextIndex++;
      if (m_currentTextIndex < m_currentTexts.Count) {
        m_textJObject = m_currentTexts[m_currentTextIndex].Value<JObject>();
        if (m_textJObject.ContainsKey(StringMap.BG)) {
          _bgImage.sprite = await ResourceManager.LoadAssetAsync<Sprite>(
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

    private void _QuitAVGScene(float fadeTime = 0.3f) {
      /*UIManager.instance.FadeBlackMask(fadeTime, () => {
        //加载时候的图
      }, () => {
        SceneManager.UnloadSceneAsync(StringMap.AVG).completed += ao => {
          UIManager.instance.FadeBlackMask(fadeTime, () => {
            //加载时候的图
          });
        };
      });*/
    }
  }
}