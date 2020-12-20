using System;
using System.Collections.Generic;

namespace Yume.Battle {
  [Serializable]
  public class Blackboard : Dictionary<string, float> {
    #region Const
    public const string ATK = "atk";
    public const string DEF = "def";
    public const string TIMES = "times";
    public const string MAX_STACK_CNT = "max_stack_cnt";
    #endregion

    public float GetValueOrDefault(string key, float defaultValue) {
      if (ContainsKey(key)) {
        return this[key];
      } else {
        return defaultValue;
      }
    }
  }
}