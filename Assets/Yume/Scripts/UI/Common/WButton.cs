using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine;

namespace Yume.UI {
  [ExecuteInEditMode]
  public class WButton : Button {
    [SerializeField]
    private Text _textComponent;
    public Text textComponent => _textComponent;

    private string m_audioClipName = "button_sound";

    protected override void Awake() {
      base.Awake();
      _textComponent = GetComponentInChildren<Text>(true);
    }

    public override void OnPointerDown(PointerEventData eventData) {
      base.OnPointerDown(eventData);
      //TODO:play sound
    }
  }
}