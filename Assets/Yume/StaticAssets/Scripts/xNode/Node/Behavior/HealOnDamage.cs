using UnityEngine;

namespace Yume.Battle {
  public class HealOnDamage : LeafNodeBase {
    public override bool Run() {
      int healRate = behaviorTree.blackboard.GetValue<int>("healRateOnDamage");
      int lastDamageValue = owner.blackboard.GetValue<int>("lastDamageValue");
      int heal = healRate * lastDamageValue / Constant.THOUSAND;
      if (heal > 0) {
        int realHeal = owner.AddAttrib(AttribType.HP, heal);
        // 显示数字
        NumFactory.ShowNum(realHeal, owner.numReference.position, Color.green);
      }
      return true;
    }
  }
}