using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine;
using Yume.Audio;
using Yume.String;

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

    protected override void Awake() {
      base.Awake();
      _textComponent = GetComponentInChildren<Text>(true);
      ColorBlock newColors = colors;
      newColors.normalColor = Color.white;
      newColors.highlightedColor = Color.white;
      newColors.selectedColor = Color.white;
      newColors.disabledColor = Color.white;
      colors = newColors;
    }

    public override void OnPointerClick(PointerEventData eventData) {
      if (currentSelectionState != SelectionState.Disabled || _soundEvenDisabled) {
        string clipName = string.IsNullOrEmpty(_overrideSoundName) ? StringMap.DEFAULT_CLICK_SOUND : _overrideSoundName;
        AudioManager.instance.PlaySound(clipName);
      }

      base.OnPointerClick(eventData);
    }
  }
}