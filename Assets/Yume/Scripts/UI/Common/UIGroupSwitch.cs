using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Yume.UI {
  /// <summary>
  /// 在同一个父UI中属于一个Group的不同子UI之间切换
  /// 这个类只提供最基础的切换功能，如果还有其他的需要可以创建子类去扩展
  /// </summary>
  public class UIGroupSwitch : MonoBehaviour {
    [Serializable]
    private class UIKV {
      public Button button;
      public GameObject content;
      /// <summary>
      /// 注意子UI交互开关需写在动画的帧事件里
      /// 只需要一个open动画，并记得拖到clip上
      /// </summary>
      public Animation animation;
      public Sprite btnSelectedImg;
      public Sprite btnUnselectedImg;
    }

    [SerializeField]
    private List<UIKV> _uiList;
    private UIKV m_currentUI;

    private void Awake() {
      foreach (var ui in _uiList) {
        ui.content.SetActiveEx(false);
        ui.button.onClick.AddListener(() => _Switch(ui));
      }

      m_currentUI = _uiList.First();
    }

    /// <summary>
    /// 父UI打开时调用
    /// </summary>
    public void OnOpen() {
      _Switch(m_currentUI, true);
    }

    private void _Switch(UIKV ui, bool force = false) {
      if (m_currentUI == ui && !force) {
        return;
      }

      if (m_currentUI != null) {
        m_currentUI.button.image.sprite = m_currentUI.btnUnselectedImg;
        m_currentUI.content.SetActiveEx(false);
      }

      ui.button.image.sprite = ui.btnSelectedImg;
      ui.content.SetActiveEx(true);
      ui.animation.Play();

      m_currentUI = ui;
    }
  }
}