using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using Yume.String;
using Yume.Audio;
using System.ComponentModel;
using System.Collections.Generic;
using System;

namespace Yume.UI {
  public enum UISettingType {
    [Description("bgm")]
    BGM,
    [Description("sound")]
    SOUND
  }

  public class UISetting : UIBase {
    private class UISwitchAndValue {
      private UISettingType m_type;
      private Slider m_valueSlider;
      private Text m_closeText;
      private Text m_openText;
      private Button m_switchButton;
      private RectTransform m_switchSlider;
      private Tween m_tween;
      private float m_switchSliderMoveDist;
      private string valueKey => m_type.GetDescription() + StringMap.VALUE;
      private string switchKey => m_type.GetDescription() + StringMap.SWITCH;

      public UISwitchAndValue(UISettingType type, Slider valueSlider, Button switchButton,
                                   Text closeText, Text openText, RectTransform switchSlider) {

        m_type = type;
        m_openText = openText;
        m_closeText = closeText;
        m_valueSlider = valueSlider;
        m_switchSlider = switchSlider;
        m_switchButton = switchButton;

        m_switchSliderMoveDist = m_switchSlider.sizeDelta.x;
        if (PlayerPrefs.GetInt(switchKey) <= 0) {
          Color tempColor = m_closeText.color;
          m_closeText.color = m_openText.color;
          m_openText.color = tempColor;
          m_switchSlider.anchoredPosition -= new Vector2(m_switchSlider.sizeDelta.x, 0);
          m_switchSliderMoveDist *= -1.0f;
        }
        if (m_valueSlider) {
          m_valueSlider.value = PlayerPrefs.GetInt(valueKey);
        }

        _AddListener();
      }

      private void _AddListener() {
        m_switchButton.onClick.AddListener(_ChangeSwitch);
        m_valueSlider.onValueChanged.AddListener(_ChangeValue);
      }

      private void _ChangeSwitch() {
        PlayerPrefs.SetInt(switchKey, (PlayerPrefs.GetInt(switchKey) + 1) % 2);
        Color tempColor = m_closeText.color;
        m_closeText.color = m_openText.color;
        m_openText.color = tempColor;
        m_switchSliderMoveDist *= -1.0f;

        if (m_tween != null) {
          m_tween.Complete();
        }
        m_tween = m_switchSlider.DOAnchorPosX(m_switchSliderMoveDist, 0.15f).SetRelative().OnComplete(() => {
          m_tween = null;
        });
        _UpdateState();
      }

      private void _ChangeValue(float _) {
        PlayerPrefs.SetInt(valueKey, (int)m_valueSlider.value);
        _UpdateState();
      }

      private void _UpdateState() {
        switch (m_type) {
          case UISettingType.BGM: {
              AudioManager.instance.UpdateBgmVolume();
              break;
            }
          case UISettingType.SOUND: {
              AudioManager.instance.UpdateSoundVolume();
              break;
            }
        }
      }
    }

    private List<UISwitchAndValue> m_settings = new List<UISwitchAndValue>();

    public override void OnInit(UIData uiData, GameObject root) {
      base.OnInit(uiData, root);

      foreach (UISettingType type in Enum.GetValues(typeof(UISettingType))) {
        Text openText = GetComponent<Text>($"{type.GetDescription()}/{nameof(openText)}");
        Text closeText = GetComponent<Text>($"{type.GetDescription()}/{nameof(closeText)}");
        Slider valueSlider = GetComponent<Slider>($"{type.GetDescription()}/{nameof(valueSlider)}");
        Button switchButton = GetComponent<Button>($"{type.GetDescription()}/{nameof(switchButton)}");
        RectTransform switchSlider = GetComponent<RectTransform>($"{type.GetDescription()}/{nameof(switchSlider)}");
        m_settings.Add(new UISwitchAndValue(type, valueSlider, switchButton, closeText, openText, switchSlider));
      }

      GetComponent<Button>("close").onClick.AddListener(() => UIManager.instance.CloseOne("setting"));
    }
  }
}