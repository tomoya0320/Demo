using System;
using System.Collections.Generic;
using UnityEngine;

namespace Yume.Asset {
  public class GuideAsset : YScriptableObject {
    [SerializeField]
    private GuideTemplate[] _guideTemplates;

    public override Dictionary<(string, Type), object> GetActualData() {
      Dictionary<(string, Type), object> data = new Dictionary<(string, Type), object>();
      foreach (var guide in _guideTemplates) {
        data.Add((guide.id, typeof(GuideTemplate)), guide);
      }
      return data;
    }
  }
}