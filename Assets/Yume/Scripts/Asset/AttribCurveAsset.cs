using System;
using System.Collections.Generic;
using UnityEngine;
using Yume.Resource;

namespace Yume.Data {
  [Serializable]
  public struct AttribCurve {
    public string id;
    public AnimationCurve curve;
  }

  public class AttribCurveAsset : YScriptableObject {
    [SerializeField]
    private List<AttribCurve> _attribCurves;

    public List<AttribCurve> attribCurves => _attribCurves;

    public override Dictionary<string, object> GetAssets() {
      Dictionary<string, object> assets = new Dictionary<string, object>();
      assets.Add(nameof(attribCurves), attribCurves);

      return assets;
    }
  }
}