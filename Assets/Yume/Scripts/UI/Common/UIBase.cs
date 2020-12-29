using System;
using System.Collections;
using UnityEngine;
using Yume.Tool;

namespace Yume.UI {
  public abstract class UIBase {
    private UIData m_uiData;
    private bool m_isShowing;
    private GameObject m_root;
    private UIObject m_uiObject;
    private Animation m_animation;
    public GameObject root => m_root;
    private CanvasGroup m_canvasGroup;
    public string name => m_uiData.name;
    public float cacheTime => m_uiData.cacheTime;
    /// <summary>
    /// open动画开始到close动画结束期间都算显示中
    /// </summary>
    public bool isShowing => m_isShowing;

    public virtual void OnInit(UIData uiData, GameObject root) {
      m_root = root;
      m_uiData = uiData;
      m_root.name = uiData.name;
      m_uiObject = root.GetComponent<UIObject>();
      m_animation = root.GetComponent<Animation>();
      m_canvasGroup = root.GetComponent<CanvasGroup>();
    }

    protected T GetComponent<T>(string objName) where T : Component {
      return m_uiObject.GetComponent<T>(objName);
    }

    protected GameObject GetGameObject(string objName) {
      return m_uiObject.GetGameObject(objName);
    }

    /// <summary>
    /// 当没有动画时直接执行callback
    /// </summary>
    /// <param name="callback"></param>
    /// <param name="args"></param>
    public virtual void Close(Action callback = null, params object[] args) {
      //保证动画完整播放
      m_canvasGroup.interactable = false;
      UIManager.instance.activeBack = false;

      Action action = () => {
        m_isShowing = false;
        m_canvasGroup.interactable = true;
        UIManager.instance.activeBack = true;
      };
      callback = action.AddAction(callback);

      if (m_animation.GetClip("close")) {
        m_animation.Play("close");
        CoroutineHelper.instance.AnimCallback(m_animation, callback);
      } else {
        callback?.Invoke();
      }
    }

    /// <summary>
    /// 当没有动画时直接执行onOpenEnd
    /// </summary>
    /// <param name="callback"></param>
    /// <param name="args"></param>
    public virtual void Open(Action callback = null, params object[] args) {
      //保证动画完整播放
      m_isShowing = true;
      m_canvasGroup.interactable = false;
      UIManager.instance.activeBack = false;

      Action action = () => {
        m_canvasGroup.interactable = true;
        UIManager.instance.activeBack = true;
      };
      callback = action.AddAction(callback);

      if (m_animation.GetClip("open")) {
        m_animation.Play("open");
        CoroutineHelper.instance.AnimCallback(m_animation, callback);
      } else {
        callback?.Invoke();
      }
    }

    /// <summary>
    /// 回收UI对象，返回UI实例
    /// </summary>
    public virtual UIBase Recycle() {
      root.transform.SetParent(UIManager.instance.cacheCanvas);
      return this;
    }

    /// <summary>
    /// 回收内存
    /// </summary>
    public virtual void Destroy() {
      UnityEngine.Object.Destroy(root);
    }
  }
}