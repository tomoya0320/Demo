using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine;
using Yume.Audio;
using Yume.String;

namespace Yume.UI {
  [ExecuteInEditMode]
  public class WButton : Button {
    [SerializeField]
    private Text _textComponent;
    public Text textComponent => _textComponent;
    [SerializeField]
    private string _overrideSoundName;

    protected override void Awake() {
      base.Awake();
      _textComponent = GetComponentInChildren<Text>(true);
    }

    public override void OnPointerDown(PointerEventData eventData) {
      base.OnPointerDown(eventData);

      string clipName = string.IsNullOrEmpty(_overrideSoundName) ? 
                       StringMap.DEFAULT_CLICK_SOUND : _overrideSoundName;

      AudioController.instance.PlaySound(clipName);
    }
  }
}