using System;
using System.Collections.Generic;
using UnityEngine;

namespace Yume.Asset {
  public class CurveAsset : YScriptableObject {
    [SerializeField]
    private Dictionary<string, AnimationCurve> _curves;
    [SerializeField]
    private AnimationCurve[] _cardMoveCurves;

    public override Dictionary<(string, Type), object> GetActualData() {
      Dictionary<(string, Type), object> data = new Dictionary<(string, Type), object>();
      foreach (var kv in _curves) {
        data.Add((kv.Key, typeof(AnimationCurve)), kv.Value);
      }
      data.Add(("cardMoveCurves", typeof(AnimationCurve[])), _cardMoveCurves);
      
      return data;
    }
  }
}