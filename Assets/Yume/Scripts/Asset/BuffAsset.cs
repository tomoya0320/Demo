using System;
using System.Collections.Generic;
using UnityEngine;
using Yume.Data;

namespace Yume.Asset {
  public class BuffAsset : YScriptableObject {
    [SerializeField]
    private BuffTemplate[] _buffs;

    public override Dictionary<(string, Type), object> GetActualData() {
      Dictionary<(string, Type), object> data = new Dictionary<(string, Type), object>();
      foreach (var buff in _buffs) {
        data.Add((buff.id, typeof(BuffTemplate)), buff);
      }
      return data;
    }
  }
}