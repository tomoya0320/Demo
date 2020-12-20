using System;
using System.Collections;
using UnityEngine;
using Yume.Tool;

namespace Yume.UI {
  /// <summary>
  /// 不进栈
  /// UI之间的开关彼此独立互不影响
  /// </summary>
  public abstract class UIBase {
    private UIData m_uiData;
    private GameObject m_root;
    private UIObject m_uiObject;
    private Animation m_animation;
    public GameObject root => m_root;
    public string name => m_uiData.name;
    public UIType uiType => m_uiData.uiType;
    public float cacheTime => m_uiData.cacheTime;

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
      onCloseStart?.Invoke();
      if (m_animation.GetClip("close")) {
        m_animation.Play("close");
        UIManager.instance.SetSharedArgs(uiType + "isTweening", true);
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
      onOpenStart?.Invoke();
      if (m_animation.GetClip("open")) {
        m_animation.Play("open");
        UIManager.instance.SetSharedArgs(uiType + "isTweening", true);
        CoroutineHelper.instance.RunCoroutine(_AnimCallBack(onOpenEnd));
      } else {
        onOpenEnd?.Invoke();
      }
    }

    private IEnumerator _AnimCallBack(Action callback) {
      yield return new WaitUntil(() => !m_animation.isPlaying);
      UIManager.instance.SetSharedArgs(uiType + "isTweening", false);
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