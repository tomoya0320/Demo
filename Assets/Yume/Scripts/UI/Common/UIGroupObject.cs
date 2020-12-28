using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Yume.UI {
  public class UIGroupObject : MonoBehaviour {
    [Serializable]
    private class UIKV {
      public Button button;
      public GameObject content;
      public Sprite btnSelectedImg;
      public Sprite btnUnselectedImg;
    }

    [SerializeField]
    private List<UIKV> _uiList;
    private UIKV m_lastUI;

    private void Awake() {
      foreach (var ui in _uiList) {
        ui.content.SetActive(false);
        ui.button.onClick.AddListener(() => _Switch(ui));
      }

      if (_uiList.First() != null) {
        _Switch(_uiList.First());
      }
    }

    private void _Switch(UIKV ui) {
      if (m_lastUI!=null) {
        m_lastUI.content.SetActive(false);
        m_lastUI.button.image.sprite = m_lastUI.btnUnselectedImg;
      }
      ui.content.SetActive(true);
      ui.button.image.sprite = ui.btnSelectedImg; ;
      m_lastUI = ui;
    }
  }
}