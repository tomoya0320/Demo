using System;
using System.Collections.Generic;
using UnityEngine;

namespace Yume.Asset {
  public class AttribCurveAsset : YScriptableObject {
    [SerializeField]
    private Dictionary<string, AnimationCurve> _attribCurves;

    public override Dictionary<(string, Type), object> GetActualData() {
      Dictionary<(string, Type), object> data = new Dictionary<(string, Type), object>();
      foreach (var kv in _attribCurves) {
        data.Add((kv.Key, typeof(AnimationCurve)), kv.Value);
      }
      return data;
    }
  }
}