using UnityEngine;

namespace Yume.Battle {
  /// <summary>
  /// 伤害公式:最终伤害=(卡牌攻击值+卡牌攻击率*攻击者的攻击力)*(1+攻击者的伤害加深率-被击者的反伤害加深率)+(攻击者的伤害加深值-被击者的反伤害加深值)
  /// </summary>
  public class PureDamage : DamageBase {
    protected override int OnDamage(Unit caster = null) {
      // 卡牌攻击值
      int cardAtk = behaviorTree.blackboard.GetValue<int>("atk");
      // 卡牌攻击率
      int cardAtkRate = behaviorTree.blackboard.GetValue<int>("atkRate");
      // 攻击者的攻击力
      int atk = caster?.GetAttrib(AttribType.ATK) ?? 0;
      // 攻击者的伤害加深值
      int damageChange = caster?.GetAttrib(AttribType.DAMAGE_CHANGE) ?? 0;
      // 攻击者的伤害加深率
      int damageChangeRate = caster?.GetAttrib(AttribType.DAMAGE_CHANGE_RATE) ?? 0;
      // 被击者的反伤害加深值
      int antiDamageChange = owner.GetAttrib(AttribType.ANTI_DAMAGE_CHANGE);
      // 被击者的反伤害加深率
      int antiDamageChangeRate = owner.GetAttrib(AttribType.ANTI_DAMAGE_CHANGE_RATE);

      // 最终的伤害
      int finalDamage = (Constant.THOUSAND * cardAtk + cardAtkRate * atk) *
                        (Constant.THOUSAND + damageChangeRate - antiDamageChangeRate) /
                        (Constant.THOUSAND * Constant.THOUSAND) +
                        (damageChange + antiDamageChange);

      return finalDamage > 0 ? Mathf.Abs(owner.AddAttrib(AttribType.HP, -finalDamage)) : 0;
    }
  }
}