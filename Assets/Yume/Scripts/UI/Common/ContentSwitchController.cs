using UnityEngine;
using UnityEngine.UI;

namespace Yume.UI {
  public class ContentSwitchController : MonoBehaviour {
    [SerializeField]
    private Sprite _btnSelectedImg;
    [SerializeField]
    private Sprite _btnUnselectedImg;
    [SerializeField]
    private Button _lastSelectedButton;
    [SerializeField]
    private GameObject _lastSelectedContent;

    public void Switch(Button button) {
      if (_lastSelectedButton) {
        _lastSelectedButton.image.sprite = _btnUnselectedImg;
      }
      button.image.sprite = _btnSelectedImg;
      _lastSelectedButton = button;
    }

    public void Switch(GameObject content) {
      if (_lastSelectedContent) {
        _lastSelectedContent.SetActive(false);
      }
      content.SetActive(true);
      _lastSelectedContent = content;
    }
  }
}