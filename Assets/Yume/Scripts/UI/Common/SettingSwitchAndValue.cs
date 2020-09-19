using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using Yume.String;
using Yume.Audio;

namespace Yume.UI {
  public enum SettingType {
    BGM, SOUND
  }

  public class SettingSwitchAndValue : MonoBehaviour {
    [SerializeField]
    private SettingType _settingType;
    [SerializeField]
    private Slider _valueSlider;
    [SerializeField]
    private Text _closeText;
    [SerializeField]
    private Text _openText;
    [SerializeField]
    private RectTransform _switchSlider;
    private Tween m_tween;
    private float m_switchSliderMoveDist;
    private string valueKey => _settingType.ToString() + StringMap.VALUE;
    private string switchKey => _settingType.ToString() + StringMap.SWITCH;

    private void Awake() {
      m_switchSliderMoveDist = _switchSlider.sizeDelta.x;
      if (PlayerPrefs.GetInt(switchKey) <= 0) {
        Color tempColor = _closeText.color;
        _closeText.color = _openText.color;
        _openText.color = tempColor;
        _switchSlider.anchoredPosition -= new Vector2(_switchSlider.sizeDelta.x, 0);
        m_switchSliderMoveDist *= -1.0f;
      }
      if (_valueSlider) {
        _valueSlider.value = PlayerPrefs.GetInt(valueKey);
      }
    }

    public void ChangeSwitch() {
      PlayerPrefs.SetInt(switchKey, (PlayerPrefs.GetInt(switchKey) + 1) % 2);
      Color tempColor = _closeText.color;
      _closeText.color = _openText.color;
      _openText.color = tempColor;
      m_switchSliderMoveDist *= -1.0f;

      if (m_tween != null) {
        m_tween.Complete();
      }
      m_tween = _switchSlider.DOAnchorPosX(m_switchSliderMoveDist, 0.15f).SetRelative().OnComplete(() => {
        m_tween = null;
      });
      _UpdateState();
    }

    public void ChangeValue() {
      PlayerPrefs.SetInt(valueKey, (int)_valueSlider.value);
      _UpdateState();
    }

    private void _UpdateState() {
      switch (_settingType) {
        case SettingType.BGM: {
            AudioController.instance.UpdateBgmVolume();
            break;
          }
        case SettingType.SOUND: {
            AudioController.instance.UpdateSoundVolume();
            break;
          }
      }
    }
  }
}