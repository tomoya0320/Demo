using System;
using System.Collections.Generic;
using UnityEngine;

namespace Yume.Asset {
  public class UIAsset : YScriptableObject {
    [SerializeField]
    private UITemplate[] _uiTemplates;

    public override Dictionary<(string, Type), object> GetActualData() {
      Dictionary<(string, Type), object> data = new Dictionary<(string, Type), object>();
      foreach (var ui in _uiTemplates) {
        data.Add((ui.name, typeof(UITemplate)), ui);
      }
      return data;
    }
  }
}