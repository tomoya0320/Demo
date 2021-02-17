using UnityEngine;

namespace Yume.Battle {
  /// <summary>
  /// 真实伤害不需要计算对方的防御
  /// </summary>
  public class PureDamage : DamageBase {
    protected override float OnDamage(Unit caster) {
      // blackboard中的攻击比率
      float finalAtk = behaviorTree.blackboard.GetValue<float>("atk");
      float atkRate = behaviorTree.blackboard.GetValue<float>("atkRate");
      if (caster != null) {
        // 计算攻击发起者的基础攻击力
        finalAtk += caster.GetAttrib(AttribType.ATK) * atkRate;
        // 计算攻击前的附加攻击比率
        finalAtk *= (1.0f + caster.blackboard.GetValueAndRemove<float>("extraAtkRate"));
        // 计算攻击前的附加攻击值
        finalAtk += caster.blackboard.GetValueAndRemove<float>("extraAtk");
      }
      // 伤害本身值应该是非负数
      return Mathf.Abs(behaviorTree.unit.AddAttrib(AttribType.HP, -finalAtk));
    }
  }
}