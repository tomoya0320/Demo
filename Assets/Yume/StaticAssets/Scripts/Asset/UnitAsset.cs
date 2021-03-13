using System;
using System.Collections.Generic;
using UnityEngine;
using Yume.Data;

namespace Yume.Asset {
  public class UnitAsset : YScriptableObject {
    [SerializeField]
    private UnitTemplate[] _unitTemplates;

    public override Dictionary<(string, Type), object> GetActualData() {
      Dictionary<(string, Type), object> data = new Dictionary<(string, Type), object>();
      foreach (var unit in _unitTemplates) {
        data.Add((unit.id, typeof(UnitTemplate)), unit);
      }
      return data;
    }
  }
}