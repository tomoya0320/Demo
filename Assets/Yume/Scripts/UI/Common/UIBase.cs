using System;
using UnityEngine;
using Yume.Data;
using Object = UnityEngine.Object;

namespace Yume.UI {
  public abstract class UIBase {
    private GameObject m_root;
    private UITemplate m_uiData;
    private Animation m_animation;
    public GameObject root => m_root;
    private ObjectContainer m_uiObject;
    public string name => m_uiData.name;
    public UIType uiType => m_uiData.uiType;
    public float cacheTime => m_uiData.cacheTime;

    public virtual void OnInit(UITemplate uiTemplate, GameObject gameObject) {
      m_root = gameObject;
      m_uiData = uiTemplate;
      m_root.name = uiTemplate.name;
      m_animation = gameObject.GetComponent<Animation>();
      m_uiObject = gameObject.GetComponent<ObjectContainer>();
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
      UIManager.Block(this);

      Action action = () => {
        UIManager.Unblock(this);
      };
      callback = action + callback;

      var closeClip = m_animation.GetClip("close");
      if (closeClip != null) {
        CoroutineHelper.instance.PlayAnimCallback(closeClip, m_animation, callback);
      } else {
        callback();
      }
    }

    /// <summary>
    /// 当没有动画时直接执行onOpenEnd
    /// </summary>
    /// <param name="callback"></param>
    /// <param name="args"></param>
    public virtual void Open(Action callback = null, params object[] args) {
      //保证动画完整播放
      UIManager.Block(this);

      Action action = () => {
        UIManager.Unblock(this);
      };
      callback = action + callback;

      var opneClip = m_animation.GetClip("open");
      if (opneClip != null) {
        CoroutineHelper.instance.PlayAnimCallback(opneClip, m_animation, callback);
      } else {
        callback();
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
      Object.Destroy(root);
    }
  }

  /// <summary>
  /// 不受UIManager管理，由父UI管理
  /// </summary>
  public abstract class UIChildBase {
    private GameObject m_root;
    private ObjectContainer m_uiObject;
    public GameObject gameObject => m_root;

    public UIChildBase(GameObject root) {
      m_root = root;
      m_uiObject = root.GetComponent<ObjectContainer>();
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