using System.Collections.Generic;

namespace Yume.Battle {
  public class Blackboard : Dictionary<string, object> {
    public const string ATK = "atk";
    public const string ATK_RATE = "atkRate";
    public const string ATTRIB_ADDITIVES = "attribAdditives";
    public const string REAL_ATTRIB_ADDITIVES = "realAttribAdditives";
    public const string CASTER = "caster";
    public const string HURT_SOUND = "hurtSound";
    public const string LAST_DAMAGE_VALUE = "lastDamageValue";
    public const string LAST_DAMAGED_VALUE = "lastDamagedValue";
    public const string BUFFS = "buffs";
    public const string HEAL_RATE_ON_DAMAGE = "healRateOnDamage";
    public const string PROB = "prob";
    public const string CARD_SELECTOR = "cardSelector";
    
    public T GetValue<T>(string key, T defaultValue = default) {
      if (ContainsKey(key) && this[key] is T) {
        return (T)this[key];
      } else {
        return defaultValue;
      }
    }
    
    public bool TryGetValue<T>(string key, out T value) {
      if (ContainsKey(key) && this[key] is T) {
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
      if (ContainsKey(key) && this[key] is T) {
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