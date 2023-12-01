using UnityEngine;

namespace Yume.Battle {
  public abstract class DamageBase : LeafNodeBase {
    public sealed override bool Run() {
      if (owner.isDead) {
        return false;
      }
      // 小心不要套娃了
      BattleManager.behaviorManagers.TryGetValue(owner, out var ownerBehaviorManager);
      ownerBehaviorManager?.Run(BehaviorTime.ON_BEFORE_DAMAGED);

      // 发起者为空表示这个是由关卡加上的
      Unit caster = behaviorTree.blackboard.GetValue<Unit>(Blackboard.CASTER);
      // 造成伤害前的行为树 注意caster可能为空所以要用自定义的方法
      BattleManager.behaviorManagers.TryGetValueEx(caster, out var castBehaviorManager);
      castBehaviorManager?.Run(BehaviorTime.ON_BEFORE_DAMAGE);

      int realDamage = OnDamage(caster);

      // 播放受击音效
      string hurtSound = behaviorTree.blackboard.GetValue(Blackboard.HURT_SOUND, StringMap.DEFAULT_HURT_SOUND);
      AudioManager.PlaySound(hurtSound);
      // 显示数字
      NumFactory.ShowNum(-realDamage, owner.numReference.position, Color.red);

      // 保存最后一次造成的伤害值和伤害目标
      if (caster != null) {
        caster.blackboard[Blackboard.LAST_DAMAGE_VALUE] = realDamage;
        caster.blackboard[Blackboard.LAST_DAMAGE_TARGET] = owner;
      }
      // 保存最后一次被造成的伤害值和伤害来源
      owner.blackboard[Blackboard.LAST_DAMAGED_VALUE] = realDamage;
      owner.blackboard[Blackboard.LAST_DAMAGE_SOURCE] = caster;

      // 被造成伤害后的行为树
      ownerBehaviorManager?.Run(BehaviorTime.ON_LATE_DAMAGED);
      // 造成伤害后的行为树 
      castBehaviorManager?.Run(BehaviorTime.ON_LATE_DAMAGE);

      if (owner.isAlive) {
        // 受伤
        owner.animator.SetTrigger(AnimatorParams.HURT);
      } else {
        ownerBehaviorManager?.Run(BehaviorTime.ON_BEFORE_DIE);
      }

      // 在即将死亡前执行行为树有可能会脱离死亡状态所以要再判断一次
      if (owner.isDead) {
        // 单位死亡
        BattleManager.OnUnitDie(owner);
      }

      return true;
    }

    protected abstract int OnDamage(Unit caster = null);
  }
}