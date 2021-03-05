using UnityEngine;

namespace Yume.Battle {
  public class NormalDamage : DamageBase {
    protected override float OnDamage(Unit caster = null) {
      Unit owner = behaviorTree.unit;
      // blackboard中的攻击值
      float finalAtk = behaviorTree.blackboard.GetValue<float>("atk");
      // blackboard中的攻击比率
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
      finalAtk -= (1.0f + owner.blackboard.GetValueAndRemove<float>("extraDefRate")) * owner.GetAttrib(AttribType.DEF);
      // 计算自己被攻击前的防御值
      finalAtk -= owner.blackboard.GetValueAndRemove<float>("extraDef");
      
      if (finalAtk > 0) {
        return Mathf.Abs(owner.AddAttrib(AttribType.HP, -finalAtk));
      } else {
        return 0;
      }
    }
  }
}