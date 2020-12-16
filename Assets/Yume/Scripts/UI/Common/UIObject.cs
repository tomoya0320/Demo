using System;
using System.Collections.Generic;
using UnityEngine;

namespace Yume.UI {
  public class UIObject : MonoBehaviour {
    [Serializable]
    private class UIKV {
      public string name;
      public GameObject gameObject;
    }

    [SerializeField]
    private List<UIKV> _uiList;

    public T GetComponent<T>(string objName) where T : Component {
      var uiKV = _uiList.Find(ui => ui.name == objName);
      if (uiKV == null || uiKV.gameObject == null) {
        return null;
      }
      var component = uiKV.gameObject.GetComponent<T>();
      return component ?? uiKV.gameObject.AddComponent<T>();
    }
  }
}