using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace Yume {
  public class ObjectContainer : SerializedMonoBehaviour {
    [SerializeField, DictionaryDrawerSettings(KeyLabel = "名称", ValueLabel = "对象引用")]
    private Dictionary<string, GameObject> _objectDict;

    public T GetComponent<T>(string objName) where T : Component {
      var obj = GetGameObject(objName);
      if (obj == null) {
        return null;
      }
      var component = obj.GetComponent<T>();
      return component ? component : obj.AddComponent<T>();
    }

    public GameObject GetGameObject(string objName) {
      return _objectDict.ContainsKey(objName) ? _objectDict[objName] : null;
    }
  }
}