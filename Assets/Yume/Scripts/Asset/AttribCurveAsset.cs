using Newtonsoft.Json;
using UnityEngine;
using System;

namespace Yume.DB {
  [Serializable]
  public struct AttribCurve {
    public string id;
    public AnimationCurve curve;
  }

  //因为ScriptableObject父类里有些其他不需要的字段所以需要反向剔除
  [JsonObject(MemberSerialization.OptIn)]
  public class AttribCurveAsset : ScriptableObject {
    [JsonProperty]
    public AttribCurve[] attribCurves;
  }
}