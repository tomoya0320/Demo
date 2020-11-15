using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Yume.Battle {
  [Serializable]
  public struct Blackboard {
    #region Const
    public const string ATK = "atk";
    public const string DEF = "def";
    public const string TIMES = "times";
    public const string MAX_STACK_CNT = "max_stack_cnt";
    #endregion
    [JsonProperty("data")]
    private Dictionary<string, float> m_data;

    public float TryGetValueOrDefault(string key,float defaultValue) {
      if (m_data.ContainsKey(key)) {
        return m_data[key];
      } else {
        return defaultValue;
      }
    }
  }
}