using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Yume.UI {
  public class UIGroupSwitch : MonoBehaviour {
    [Serializable]
    public class UIContent {
      public Button button;
      public GameObject content;
      public Animation animation;
    }

    private UIContent m_currentUI;
    [SerializeField]
    private Sprite _btnSelectedImg;
    [SerializeField]
    private Sprite _btnUnselectedImg;
    [SerializeField]
    private List<UIContent> _uiList;

    private void Awake() {
      foreach (var ui in _uiList) {
        ui.content.SetActiveEx(false);
        ui.button.onClick.AddListener(() => Switch(ui));
      }

      m_currentUI = _uiList.First();
    }

    /// <summary>
    /// 父UI打开时调用主要为了重新播放动画
    /// </summary>
    public void OnOpen() {
      Switch(_uiList.First(), true);
    }

    public void Switch(UIContent ui, bool force = false) {
      if (m_currentUI == ui && !force) {
        return;
      }

      if (m_currentUI != null) {
        m_currentUI.button.image.sprite = _btnUnselectedImg;
        m_currentUI.content.SetActiveEx(false);
      }

      ui.button.image.sprite = _btnSelectedImg;
      ui.content.SetActiveEx(true);
      ui.animation.Play();

      m_currentUI = ui;
    }
  }
}