using System;
using System.ComponentModel;
using UnityEngine;

namespace Yume.Battle {
  /// <summary>
  /// Description用来存储属性的实际显示名称
  /// </summary>
  public enum AttribType {
    [Description("生命")]
    HP,
    [Description("攻击")]
    ATK,
    /// <summary>
    /// 不显示
    /// </summary>
    [Description("防御")]
    DEF,
    [Description("能量")]
    ENERGY,
  }

  [Serializable]
  public struct Attrib {
    [SerializeField]
    private float _value;
    public float value => _value;
    [SerializeField]
    private float _maxValue;
    public float maxValue => _maxValue;
    [SerializeField]
    private bool _allowNegative;  
    [SerializeField]
    private bool _allowExceedMax;


    #region 仅为了方便根据等级计算属性

    public static Attrib operator +(Attrib a1, Attrib a2) {
      Attrib attrib = new Attrib {
        _value = a1._value + a2._value,
        _maxValue = a1._maxValue + a2._maxValue,
        _allowNegative = a1._allowNegative || a2._allowNegative,
        _allowExceedMax = a1._allowExceedMax || a2._allowExceedMax
      };
      return attrib;
    }

    public static Attrib operator -(Attrib a1, Attrib a2) {
      Attrib attrib = new Attrib {
        _value = a1._value - a2._value,
        _maxValue = a1._maxValue - a2._maxValue,
        _allowNegative = a1._allowNegative || a2._allowNegative,
        _allowExceedMax = a1._allowExceedMax || a2._allowExceedMax
      };
      return attrib;
    }

    public static Attrib operator *(Attrib a, float value) {
      Attrib attrib = new Attrib {
        _value = value * a._value,
        _maxValue = value * a._maxValue,
        _allowNegative = a._allowNegative,
        _allowExceedMax = a._allowExceedMax
      };
      return attrib;
    }

    public static Attrib operator *(float value, Attrib a) {
      Attrib attrib = new Attrib {
        _value = value * a._value,
        _maxValue = value * a._maxValue,
        _allowNegative = a._allowNegative,
        _allowExceedMax = a._allowExceedMax
      };
      return attrib;
    }

    #endregion
    
    /// <summary>
    /// 添加值
    /// </summary>
    /// <param name="value">可正可负</param>
    /// <returns>返回实际添加的值</returns>
    public float AddValue(float value) {
      float origin = _value;
      _value += value;
      if (_value < 0 && !_allowNegative) {
        _value = 0;
      }
      if (_value > _maxValue && !_allowExceedMax) {
        _value = _maxValue;
      }
      return _value - origin;
    }

    /// <summary>
    /// 添加最大值
    /// </summary>
    /// <param name="maxValue">可正可负</param>
    /// <returns>返回实际添加的最大值</returns>
    public float AddMaxValue(float maxValue) {
      float origin = _maxValue;
      // 最大值不能为负
      _maxValue = Mathf.Max(0, _maxValue + maxValue);
      if (_maxValue > origin) {
        AddValue(_maxValue - origin);
      } else if (_maxValue < _value) {
        AddValue(_maxValue - _value);
      }
      return _maxValue - origin;
    }

    public override string ToString() {
      return $"value:{_value},maxValue:{_maxValue}";
    }
  }
}