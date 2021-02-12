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
    [Description("能量")]
    ENERGY,
  }

  [Serializable]
  public struct Attrib {
    [SerializeField]
    private float m_value;
    public float value => m_value;
    [SerializeField]
    private float m_maxValue;
    public float maxValue => m_maxValue;
    [SerializeField]
    private bool m_allowNegative;


    #region 仅为了方便根据等级计算属性
    public static Attrib operator +(Attrib a1, Attrib a2) {
      Attrib attrib = new Attrib {
        m_value = a1.m_value + a2.m_value,
        m_maxValue = a1.m_maxValue + a2.m_maxValue
      };
      return attrib;
    }

    public static Attrib operator -(Attrib a1, Attrib a2) {
      Attrib attrib = new Attrib {
        m_value = a1.m_value - a2.m_value,
        m_maxValue = a1.m_maxValue - a2.m_maxValue
      };
      return attrib;
    }

    public static Attrib operator *(Attrib a, float value) {
      Attrib attrib = new Attrib {
        m_value = value * a.m_value,
        m_maxValue = value * a.m_maxValue
      };
      return attrib;
    }

    public static Attrib operator *(float value, Attrib a) {
      Attrib attrib = new Attrib {
        m_value = value * a.m_value,
        m_maxValue = value * a.m_maxValue
      };
      return attrib;
    }
    #endregion

    #region 一场战斗中永久性增加的时候调用
    public void AddValue(float value) {
      m_value += value;
      if (!m_allowNegative && m_value < 0) {
        m_value = 0;
      }
    }

    public void AddMaxValue(float maxValue) {
      float orign = m_maxValue;
      m_maxValue = Mathf.Max(0, m_maxValue + maxValue);
      if (m_maxValue > orign) {
        AddValue(m_maxValue - orign);
      } else if (m_maxValue < m_value) {
        AddValue(m_maxValue - m_value);
      }
    }
    #endregion

    public override string ToString() {
      return $"value:{m_value},maxValue:{m_maxValue}";
    }
  }
}