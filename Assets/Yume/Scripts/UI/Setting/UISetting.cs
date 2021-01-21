using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using Yume.Audio;
using System.ComponentModel;
using System.Collections.Generic;
using System;
using Yume.Data;

namespace Yume.UI {
  public enum UISettingType {
    [Description("bgm")]
    BGM,
    [Description("sound")]
    SOUND
  }

  public class UISetting : UIBase {
    /// <summary>
    /// SV指Switch和Value
    /// </summary>
    private class UISettingSV : UIChildBase {
      private Tween m_tween;
      private Text m_openText;
      private Text m_closeText;
      private UISettingType m_type;
      private Slider m_valueSlider;
      private Button m_switchButton;
      private float m_switchSliderMoveDist;
      private RectTransform m_switchRectTransform;
      private string valueKey => m_type.GetDescription() + StringMap.VALUE;
      private string switchKey => m_type.GetDescription() + StringMap.SWITCH;

      public UISettingSV(UISettingType type, GameObject root) : base(root) {
        m_type = type;
        m_openText = GetComponent<Text>("OpenText");
        m_closeText = GetComponent<Text>("CloseText");
        m_valueSlider = GetComponent<Slider>("ValueSlider");
        m_switchButton = GetComponent<Button>("SwitchButton");
        m_switchRectTransform = GetComponent<RectTransform>("SwitchRectTransform");

        m_switchSliderMoveDist = m_switchRectTransform.sizeDelta.x;
        if (PlayerPrefs.GetInt(switchKey) <= 0) {
          Color tempColor = m_closeText.color;
          m_closeText.color = m_openText.color;
          m_openText.color = tempColor;
          m_switchRectTransform.anchoredPosition -= new Vector2(m_switchRectTransform.sizeDelta.x, 0);
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
        m_tween = m_switchRectTransform.DOAnchorPosX(m_switchSliderMoveDist, 0.15f).SetRelative().OnComplete(() => {
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

    private UIGroupSwitch m_settingGroupSwitch;
    private List<UISettingSV> m_settings = new List<UISettingSV>();

    public override void OnInit(UIData uiData, GameObject root) {
      base.OnInit(uiData, root);

      m_settingGroupSwitch = GetComponent<UIGroupSwitch>("setting group");

      foreach (UISettingType type in Enum.GetValues(typeof(UISettingType))) {
        m_settings.Add(new UISettingSV(type, GetGameObject(type.GetDescription())));
      }

      _AddListener();
    }

    private void _AddListener() {
      GetComponent<Button>("SaveData").onClick.AddListener(() => {
        DataManager.instance.SaveData(() => {
          MessageManager.instance.AddMessage(new Message(Message.Style.FADE, StringMap.SAVE_DATA_SUCCEED));
        });
      });
      GetComponent<Button>("close").onClick.AddListener(() => UIManager.instance.CloseOne(name));
    }

    public override void Open(Action callback = null, params object[] args) {
      base.Open(callback, args);

      m_settingGroupSwitch.OnOpen();
    }
  }
}