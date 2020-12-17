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
    private bool _muteWhenInactive;

    protected override void Awake() {
      base.Awake();
      _textComponent = GetComponentInChildren<Text>(true);
      colors = new ColorBlock {
        normalColor = Color.white,
        highlightedColor = Color.white,
        pressedColor = Color.white,
        selectedColor = Color.white,
        disabledColor = Color.white,
        colorMultiplier = 1.0f,
        fadeDuration = 0
      };
    }

    public override void OnPointerDown(PointerEventData eventData) {
      base.OnPointerDown(eventData);

      if (interactable || !_muteWhenInactive) {
        string clipName = string.IsNullOrEmpty(_overrideSoundName) ?
                         StringMap.DEFAULT_CLICK_SOUND : _overrideSoundName;

        AudioManager.instance.PlaySound(clipName);
      }
    }
  }
}