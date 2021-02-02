using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;
using Yume.Data;

namespace Yume.Asset {
  public class LevelAsset : YScriptableObject {
    [Serializable]
    private class Level {
      public LevelData data;
      public LevelTemplate template;
    }

    [SerializeField]
    private Level[] _levels;

    public override Dictionary<(string, Type), object> GetActualData() {
      Dictionary<(string, Type), object> data = new Dictionary<(string, Type), object>();
      foreach (var level in _levels) {
        var levelDataJson = JsonConvert.SerializeObject(level.data);
        var levelData = JsonConvert.DeserializeObject<LevelData>(levelDataJson);
        data.Add((level.data.id, typeof(LevelData)), levelData);
        data.Add((level.template.id, typeof(LevelTemplate)), level.template);
      }
      return data;
    }
  }
}