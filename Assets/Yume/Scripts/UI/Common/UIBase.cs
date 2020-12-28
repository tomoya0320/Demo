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
      m_uiObject = root.GetComponentInChildren<UIObject>();
      m_animation = root.GetComponentInChildren<Animation>();
    }

    protected T GetComponent<T>(string objName) where T : Component {
      return m_uiObject.GetComponent<T>(objName);
    }

    protected GameObject GetGameObject(string objName) {
      return m_uiObject.GetGameObject(objName);
    }

    /// <summary>
    /// 当没有动画时直接执行onCloseEnd
    /// </summary>
    /// <param name="onCloseStart"></param>
    /// <param name="onCloseEnd"></param>
    /// <param name="args"></param>
    public virtual void Close(Action onCloseStart = null, Action onCloseEnd = null, params object[] args) {
      Action action = () => {
        UIManager.instance.SetSharedArgs(m_uiData.uiType + "isTweening", true);
      };
      onCloseStart = action.AddAction(onCloseStart);

      action = () => {
        m_isShowing = false;
        UIManager.instance.SetSharedArgs(m_uiData.uiType + "isTweening", false);
      };
      onCloseEnd = action.AddAction(onCloseEnd);

      onCloseStart?.Invoke();
      if (m_animation.GetClip("close")) {
        m_animation.Play("close");
        CoroutineHelper.instance.RunCoroutine(_AnimCallBack(onCloseEnd));
      } else {
        onCloseEnd?.Invoke();
      }
    }

    /// <summary>
    /// 当没有动画时直接执行onOpenEnd
    /// </summary>
    /// <param name="onCloseStart"></param>
    /// <param name="onCloseEnd"></param>
    /// <param name="args"></param>
    public virtual void Open(Action onOpenStart = null, Action onOpenEnd = null, params object[] args) {
      Action action = () => {
        m_isShowing = true;
        UIManager.instance.SetSharedArgs(m_uiData.uiType + "isTweening", true);
      };
      onOpenStart = action.AddAction(onOpenStart);

      action = () => {
        UIManager.instance.SetSharedArgs(m_uiData.uiType + "isTweening", false);
      };
      onOpenEnd = action.AddAction(onOpenEnd);

      onOpenStart?.Invoke();
      if (m_animation.GetClip("open")) {
        m_animation.Play("open");
        CoroutineHelper.instance.RunCoroutine(_AnimCallBack(onOpenEnd));
      } else {
        onOpenEnd?.Invoke();
      }
    }

    private IEnumerator _AnimCallBack(Action callback) {
      yield return new WaitUntil(() => !m_animation.isPlaying);
      callback?.Invoke();
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