using System;
using System.Collections.Generic;

namespace Yume.Battle {
  [Serializable]
  public class Blackboard : Dictionary<string, object> {
    #region Const
    public const string ATK = "atk";
    public const string DEF = "def";
    public const string TIMES = "times";
    public const string MAX_STACK_CNT = "max_stack_cnt";
    #endregion

    public T GetValue<T>(string key, T defaultValue = default) {
      if (ContainsKey(key)) {
        return (T)this[key];
      } else {
        return defaultValue;
      }
    }
  }
}