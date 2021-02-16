namespace Yume.Battle {
  /// <summary>
  /// 根据上一次造成的伤害回复一定量的生命
  /// </summary>
  public class HealOnDamage : LeafNodeBase {
    public override bool Run() {
      Unit owner = behaviorTree.unit;
      float healRate = behaviorTree.blackboard.GetValue<float>("healRate");
      float lastDamageValue = owner.blackboard.GetValue<float>("lastDamageValue");
      float heal = healRate * lastDamageValue;
      if (heal > 0) {
        owner.AddAttrib(AttribType.HP, heal);
      }
      return true;
    }
  }
}