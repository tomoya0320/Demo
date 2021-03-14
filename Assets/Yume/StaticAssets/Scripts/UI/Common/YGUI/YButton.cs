using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine;
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
    [SerializeField]
    private bool _mute;
    public event Action onLongPress;
    public event Action onLongPressRepeat;
    public Action<PointerEventData> onClickWithPointer;
    private const float LONG_PRESS_MIN_TIME = 0.8f;
    private const float LONG_PRESS_REPEAT_DELTA_TIME = 0.4f;

    protected override void Awake() {
      base.Awake();

      this.SetDefaultState();
      _textComponent = GetComponentInChildren<Text>(true);
    }

    public override void OnPointerClick(PointerEventData eventData) {
      if (!_mute && (currentSelectionState != SelectionState.Disabled || _soundEvenDisabled)) {
        string clipName = string.IsNullOrEmpty(_overrideSoundName) ? StringMap.DEFAULT_CLICK_SOUND : _overrideSoundName;
        AudioManager.PlaySound(clipName);
      }
      
      onClickWithPointer?.Invoke(eventData);

      base.OnPointerClick(eventData);
    }

    public override void OnPointerDown(PointerEventData eventData) {
      if (onLongPress != null) {
        StartCoroutine(_LongPress());
      }
      if (onLongPressRepeat != null) {
        StartCoroutine(_LongPressRepeat());
      }
      base.OnPointerDown(eventData);
    }

    private IEnumerator _LongPress() {
      // 下一帧按钮的状态才会变成Pressed
      yield return null;

      float startTime = Time.time;
      while (Time.time < startTime + LONG_PRESS_MIN_TIME) {
        if (currentSelectionState == SelectionState.Pressed) {
          yield return null;
        } else {
          yield break;
        }
      }

      onLongPress();
    }

    private IEnumerator _LongPressRepeat() {
      // 下一帧按钮的状态才会变成Pressed
      yield return null;

      float startTime = Time.time;
      while (Time.time < startTime + LONG_PRESS_MIN_TIME) {
        if (currentSelectionState == SelectionState.Pressed) {
          yield return null;
        } else {
          yield break;
        }
      }

      while (true) {
        onLongPressRepeat();
        startTime = Time.time;
        while (Time.time < startTime + LONG_PRESS_REPEAT_DELTA_TIME) {
          if (currentSelectionState == SelectionState.Pressed) {
            yield return null;
          } else {
            yield break;
          }
        }
      }
    }
  }
}