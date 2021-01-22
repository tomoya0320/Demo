using System;
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
      return m_uiObject?.GetComponent<T>(objName);
    }

    protected GameObject GetGameObject(string objName) {
      return m_uiObject?.GetGameObject(objName);
    }

    /// <summary>
    /// 当没有动画时直接执行callback
    /// </summary>
    /// <param name="callback"></param>
    /// <param name="args"></param>
    public virtual void Close(Action callback = null, params object[] args) {
      //保证动画完整播放
      m_canvasGroup.interactable = false;
      UIManager.activeBack = false;

      Action action = () => {
        m_isShowing = false;
        m_canvasGroup.interactable = true;
        UIManager.activeBack = true;
      };
      callback = action + callback;

      var closeClip = m_animation.GetClip("close");
      if (closeClip != null) {
        CoroutineHelper.instance.PlayAnimCallback(closeClip, m_animation, callback);
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
      UIManager.activeBack = false;

      Action action = () => {
        m_canvasGroup.interactable = true;
        UIManager.activeBack = true;
      };
      callback = action + callback;

      var opneClip = m_animation.GetClip("open");
      if (opneClip != null) {
        CoroutineHelper.instance.PlayAnimCallback(opneClip, m_animation, callback);
      } else {
        callback?.Invoke();
      }
    }

    public virtual void OnUpdate() { }

    /// <summary>
    /// 回收UI对象，返回UI实例
    /// </summary>
    public virtual UIBase Recycle() {
      root.SetParent(UIManager.cacheCanvas);
      return this;
    }

    /// <summary>
    /// 回收内存
    /// </summary>
    public virtual void Destroy() {
      UnityEngine.Object.Destroy(root);
    }
  }

  /// <summary>
  /// 不受UIManager管理，由父UI管理
  /// </summary>
  public abstract class UIChildBase {
    private GameObject m_root;
    private UIObject m_uiObject;
    public GameObject gameObject => m_root;

    public UIChildBase(GameObject root) {
      m_root = root;
      m_uiObject = root.GetComponent<UIObject>();
    }

    protected T GetComponent<T>(string objName) where T : Component {
      return m_uiObject?.GetComponent<T>(objName);
    }

    protected GameObject GetGameObject(string objName) {
      return m_uiObject?.GetGameObject(objName);
    }

    /// <summary>
    /// 父UI打开时调用
    /// </summary>
    /// <param name="args"></param>
    public virtual void OnOpen(params object[] args) { }

    /// <summary>
    /// 父UI关闭时调用
    /// </summary>
    /// <param name="args"></param>
    public virtual void OnClose(params object[] args) { }

    public virtual void OnUpdate() { }

    public virtual void Destroy() { }
  }
}