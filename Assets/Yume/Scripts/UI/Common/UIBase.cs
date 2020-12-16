using UnityEngine;

namespace Yume.UI {
  public abstract class UIBase {
    protected UIData uiData;
    protected GameObject root;
    protected UIObject uiObject;
    public string name => uiData.name;

    public virtual void OnInit(UIData uiData, GameObject root) {
      this.root = root;
      this.uiData = uiData;
      uiObject = root.GetComponent<UIObject>();
    }

    public abstract void Open(params object[] paramArray);
    public abstract void Close(params object[] paramArray);
  }
}