using System;
using System.Collections.Generic;
using UnityEngine;

namespace Yume.Asset {
  public class LvMaxApMapAsset : YScriptableObject {
    [SerializeField]
    private Dictionary<int, int> _lvMaxApMap;

    public override Dictionary<(string, Type), object> GetActualData() {
      Dictionary<(string, Type), object> data = new Dictionary<(string, Type), object>();
      foreach (var kv in _lvMaxApMap) {
        data.Add((kv.Key.ToString(), typeof(int)), kv.Value);
      }
      return data;
    }
  }
}