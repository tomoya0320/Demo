using UnityEngine;
using UnityEngine.UI;

namespace Yume.UI {
  public class UIBtnSwitch : MonoBehaviour {
    private Button m_currentBtn;
    [SerializeField]
    private Sprite _btnSelectedImg;
    [SerializeField]
    private Sprite _btnUnselectedImg;

    public void Switch(Button button) {
      if (m_currentBtn == button) {
        return;
      }

      if (m_currentBtn != null) {
        m_currentBtn.image.sprite = _btnUnselectedImg;
      }

      button.image.sprite = _btnSelectedImg;
      m_currentBtn = button;
    }
  }
}