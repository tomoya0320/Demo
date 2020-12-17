using System.Collections.Generic;
using UnityEngine;

namespace Yume.UI {
  /// <summary>
  /// 不进栈
  /// UI之间的开关彼此独立互不影响
  /// </summary>
  public abstract class UIBase {
    private UIData m_uiData;
    private GameObject m_root;
    private UIObject m_uiObject;
    public GameObject root => m_root;
    public string name => m_uiData.name;
    public UIType uiType => m_uiData.uiType;
    public float cacheTime => m_uiData.cacheTime;
    private List<UIBase> m_children = new List<UIBase>();

    public virtual void OnInit(UIData uiData, GameObject root) {
      m_root = root;
      m_uiData = uiData;
      m_root.name = uiData.name;
      m_uiObject = root.GetComponent<UIObject>();
    }

    protected T GetComponent<T>(string objName) where T : Component {
      return m_uiObject.GetComponent<T>(objName);
    }

    protected GameObject GetGameObject(string objName) {
      return m_uiObject.GetGameObject(objName);
    }

    public void AddChild(UIBase child) {
      if (m_children.Contains(child)) {
        Debug.LogWarning($"子UI已添加！name:{name},child:{child.name}");
      } else {
        m_children.Add(child);
      }
    }

    public UIBase FindChild(string name) {
      return m_children.Find(ui => ui.name == name);
    }

    public virtual void Open(params object[] args) {

    }

    public virtual void Close(params object[] args) {
      foreach (var child in m_children) {
        child.Close(args);
      }
    }

    /// <summary>
    /// 回收进缓存时调用
    /// </summary>
    /// <param name="args"></param>
    public virtual void Recycle(params object[] args) {
      root.transform.SetParent(UIManager.instance.cacheCanvas);
    }

    /// <summary>
    /// 从缓存中移除时调用
    /// </summary>
    public virtual void Destroy() {
      Object.Destroy(root);
    }
  }
}