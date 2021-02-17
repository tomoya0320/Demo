using UnityEngine;

namespace Yume.Battle {
  public class NormalDamage : DamageBase {
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
      // 计算自己被攻击前的防御比率
      finalAtk -= (1.0f + behaviorTree.unit.blackboard.GetValueAndRemove<float>("extraDefRate")) * behaviorTree.unit.GetAttrib(AttribType.DEF);
      // 计算自己被攻击前的防御值
      finalAtk -= behaviorTree.unit.blackboard.GetValueAndRemove<float>("extraDef");
      // 伤害本身值应该是非负数
      return Mathf.Abs(behaviorTree.unit.AddAttrib(AttribType.HP, -finalAtk));
    }
  }
}