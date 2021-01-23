using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine;
using Yume.Audio;
using System;
using System.Collections;

namespace Yume.UI {
  [ExecuteInEditMode]
  public class YButton : Button {
    [SerializeField]
    private Text _textComponent;
    public Text textComponent => _textComponent;
    [SerializeField]
    private string _overrideSoundName;
    [SerializeField]
    private bool _soundEvenDisabled;
    public event Action onLongPress;
    private float m_longPressMinTime = 1.0f;

    protected override void Awake() {
      base.Awake();
      _textComponent = GetComponentInChildren<Text>(true);

      ColorBlock newColors = colors;
      newColors.disabledColor = Color.white;
      colors = newColors;

      Navigation newNavigation = navigation;
      newNavigation.mode = Navigation.Mode.None;
      navigation = newNavigation;
    }

    public override void OnPointerClick(PointerEventData eventData) {
      if (currentSelectionState != SelectionState.Disabled || _soundEvenDisabled) {
        string clipName = string.IsNullOrEmpty(_overrideSoundName) ? StringMap.DEFAULT_CLICK_SOUND : _overrideSoundName;
        AudioManager.instance.PlaySound(clipName);
      }

      base.OnPointerClick(eventData);
    }

    public override void OnPointerDown(PointerEventData eventData) {
      StartCoroutine(_LongPress());

      base.OnPointerDown(eventData);
    }

    private IEnumerator _LongPress() {
      yield return null;

      float startTime = Time.time;
      while(Time.time < startTime + m_longPressMinTime) {
        if (currentSelectionState == SelectionState.Pressed) {
          yield return null;
        } else {
          yield break;
        }
      }

      onLongPress?.Invoke();
    }
  }
}