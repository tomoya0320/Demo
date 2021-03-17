using UnityEngine;

namespace Yume.Battle {
  /// <summary>
  /// 根据caster上一次造成的伤害回复caster一定量的生命
  /// </summary>
  public class HealOnCasterDamage : LeafNodeBase {
    public override bool Run() {
      Unit caster = behaviorTree.blackboard.GetValue<Unit>("caster");
      if (caster == null) {
        return false;
      }
      int healRate = behaviorTree.blackboard.GetValue<int>("healRateOnCasterDamage");
      int lastDamageValue = caster.blackboard.GetValue<int>("lastDamageValue");
      int heal = healRate * lastDamageValue / Constant.THOUSAND;
      if (heal > 0) {
        int realHeal = caster.AddAttrib(AttribType.HP, heal);
        // 显示数字
        NumFactory.ShowNum(realHeal, caster.numReference.position, Color.green);
      }
      return true;
    }
  }
}