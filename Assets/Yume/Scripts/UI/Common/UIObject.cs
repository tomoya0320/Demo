using System;
using System.Collections.Generic;
using UnityEngine;

namespace Yume.UI {
  /// <summary>
  /// 该类不包含UI业务逻辑
  /// 只包含对象的引用
  /// </summary>
  public class UIObject : MonoBehaviour {
    [Serializable]
    private class UIKV {
      public string name;
      public GameObject gameObject;
    }

    [SerializeField]
    private List<UIKV> _uiList;  

    public T GetComponent<T>(string objName) where T : Component {
      var obj = GetGameObject(objName);
      if (obj == null) {
        return null;
      }
      var component = obj.GetComponent<T>();
      return component ?? obj.AddComponent<T>();
    }

    public GameObject GetGameObject(string objName) {
      var uiKV = _uiList.Find(ui => ui.name == objName);
      return uiKV?.gameObject;
    }
  }
}