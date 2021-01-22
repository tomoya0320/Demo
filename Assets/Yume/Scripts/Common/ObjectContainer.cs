using System;
using System.Collections.Generic;
using UnityEngine;

namespace Yume.UI {
  public class ObjectContainer : MonoBehaviour {
    [Serializable]
    public class ObjectKV {
      public string name;
      public GameObject gameObject;
    }

    [SerializeField]
    private List<ObjectKV> _objectKVList;  

    public T GetComponent<T>(string objName) where T : Component {
      var obj = GetGameObject(objName);
      if (obj == null) {
        return null;
      }
      var component = obj.GetComponent<T>();
      return component ?? obj.AddComponent<T>();
    }

    public GameObject GetGameObject(string objName) {
      var result = _objectKVList.Find(obj => obj.name == objName);
      return result?.gameObject;
    }
  }
}