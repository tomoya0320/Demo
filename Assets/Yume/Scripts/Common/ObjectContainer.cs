using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace Yume.UI {
  public class ObjectContainer : SerializedMonoBehaviour {
    [SerializeField, DictionaryDrawerSettings(KeyLabel = "Name", ValueLabel = "GameObject")]
    private Dictionary<string, GameObject> _objectDict;

    public T GetComponent<T>(string objName) where T : Component {
      var obj = GetGameObject(objName);
      if (obj == null) {
        return null;
      }
      var component = obj.GetComponent<T>();
      return component ?? obj.AddComponent<T>();
    }

    public GameObject GetGameObject(string objName) {
      if (_objectDict.ContainsKey(objName)) {
        return _objectDict[objName];
      }
      return null;
    }
  }
}