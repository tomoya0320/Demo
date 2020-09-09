using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Yume.UI {
  public class WButton : Button {
    private Audio.EventType m_eventType = Audio.EventType.UI;
    private string m_signal = "button_sound";

    public override void OnPointerDown(PointerEventData eventData) {
      base.OnPointerDown(eventData);
      //TODO:play sound
    }
  }
}