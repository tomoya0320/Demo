using System;
using System.Collections.Generic;
using UnityEngine;

namespace Yume.Data {
  [Serializable]
  public struct AttribCurve {
    public string id;
    public AnimationCurve curve;
  }

  public class AttribCurveAsset : ScriptableObject {
    [SerializeField]
    private List<AttribCurve> _attribCurves;

    public List<AttribCurve> attribCurves => _attribCurves;
  }
}