using System;
using System.Collections.Generic;
using UnityEngine;
using Yume.Data;

namespace Yume.Asset {
  public class LevelAsset : YScriptableObject {
    [SerializeField]
    private LevelTemplate[] _levelTemplates;

    public override Dictionary<(string, Type), object> GetActualData() {
      Dictionary<(string, Type), object> data = new Dictionary<(string, Type), object>();
      foreach (var levelTemplate in _levelTemplates) {
        data.Add((levelTemplate.id, typeof(LevelTemplate)), levelTemplate);
      }
      return data;
    }
  }
}