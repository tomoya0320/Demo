using System.Collections.Generic;

namespace Yume.Battle {
  public class Blackboard : Dictionary<string, object> {
    #region Key

    public const string ATK = "atk";
    public const string ATK_RATE = "atkRate";
    public const string ATTRIB_ADDITIVES = "attribAdditives";
    public const string REAL_ATTRIB_ADDITIVES = "realAttribAdditives";
    public const string CASTER = "caster";
    public const string HURT_SOUND = "hurtSound";
    public const string LAST_DAMAGE_VALUE = "lastDamageValue";
    public const string LAST_DAMAGED_VALUE = "lastDamagedValue";
    public const string CREATE_BUFFS = "createbuffs";
    public const string HEAL_RATE_ON_DAMAGE = "healRateOnDamage";
    public const string PROB = "prob";
    public const string CARD_SELECTOR = "cardSelector";
    public const string EVENT_COUNT = "eventCount";
    public const string EXTRA_EVENT_COUNT = "extraEventCount";
    public const string CONTAIN_BUFF = "containBuff";
    public const string TRIGGER_BUFF = "triggerBuff";
    public const string DURATION = "duration";
    public const string EXP = "exp";
    public const string EXTEND_BUFF = "extendBuff";
    public const string EXTEND_BUFF_DURATION = "extendBuffDuration";
    public const string REMOVE_BUFF = "removeBuff";
    public const string DISCARD_AND_DRAW_COUNT = "discardAndDrawCount";
    public const string DRAW_COUNT = "drawCount";
    public const string CARD_HEAP_COUNT_COMPARER = "cardHeapCountComparer";
    public const string RETAIN_COUNT = "retainCount";
    
    #endregion
    
    public bool ContainsKey<T>(string key) {
      return ContainsKey(key) && this[key] is T;
    }
    
    public T GetValue<T>(string key, T defaultValue = default) {
      if (ContainsKey<T>(key)) {
        return (T)this[key];
      } else {
        return defaultValue;
      }
    }
    
    public bool TryGetValue<T>(string key, out T value) {
      if (ContainsKey<T>(key)) {
        value = (T)this[key];
        return true;
      } else {
        value = default;
        return false;
      }
    }
    
    /// <summary>
    /// 相当于Pop
    /// </summary>
    /// <param name="key"></param>
    /// <param name="defaultValue"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T GetValueAndRemove<T>(string key, T defaultValue = default) {
      if (ContainsKey<T>(key)) {
        T result = (T)this[key];
        Remove(key);
        return result;
      } else {
        return defaultValue;
      }
    }

    public void Merge(Blackboard blackboard, bool overrideValue = false) {
      foreach (var kv in blackboard) {
        if (!ContainsKey(kv.Key)) {
          Add(kv.Key, kv.Value);
        } else if (overrideValue) {
          this[kv.Key] = kv.Value;
        }
      }
    }
  }
}