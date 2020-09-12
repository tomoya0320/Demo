using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine;

namespace Yume.UI {
  [ExecuteInEditMode]
  public class WButton : Button {
    [SerializeField]
    private Text _textComponent;
    public Text textComponent => _textComponent;

    private Audio.EventType m_eventType = Audio.EventType.UI;
    private string m_signal = "button_sound";

    protected override void Awake() {
      base.Awake();
      _textComponent = GetComponentInChildren<Text>();
    }

    public override void OnPointerDown(PointerEventData eventData) {
      base.OnPointerDown(eventData);
      //TODO:play sound
    }
  }
}