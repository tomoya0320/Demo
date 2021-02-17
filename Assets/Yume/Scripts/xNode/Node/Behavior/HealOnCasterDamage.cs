using System;

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
      float healRate = behaviorTree.blackboard.GetValue<float>("healRateOnCasterDamage");
      float lastDamageValue = caster.blackboard.GetValue<float>("lastDamageValue");
      float heal = healRate * lastDamageValue;
      if (heal > 0) {
        float realHeal = caster.AddAttrib(AttribType.HP, heal);
#if BETA
        string content = $"[{caster.name}]回复了[{realHeal:0}]点生命";
        BattleManager.Log(content);
#endif
      }
      return true;
    }
  }
}