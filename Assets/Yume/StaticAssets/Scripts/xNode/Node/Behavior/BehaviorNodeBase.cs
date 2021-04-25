using UnityEngine;
using XNode;

namespace Yume.Battle {
  public abstract class BehaviorNodeBase : Node {
    protected BehaviorTree behaviorTree;
    protected Unit owner => behaviorTree.unit;

    protected override void Init() {
      base.Init();
      behaviorTree = graph as BehaviorTree;
    }

    public abstract bool Run();
  }

  public abstract class LeafNodeBase : BehaviorNodeBase {
    [Input(ShowBackingValue.Never, ConnectionType.Override)]
    public int last;
  }

  public abstract class DecoratorNodeBase : BehaviorNodeBase {
    [Input(ShowBackingValue.Never, ConnectionType.Override)]
    public int last;
    [Output(ShowBackingValue.Never, ConnectionType.Override)]
    public int next;
  }

  public abstract class ControllerNodeBase : BehaviorNodeBase {
    [Input(ShowBackingValue.Never, ConnectionType.Override)]
    public int last;
    [Output(ShowBackingValue.Never)]
    public int next;
  }

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

      // 保存最后一次造成的伤害值
      if (caster != null) {
        caster.blackboard[Blackboard.LAST_DAMAGE_VALUE] = realDamage;
      }
      // 保存最后一次被造成的伤害值
      owner.blackboard[Blackboard.LAST_DAMAGED_VALUE] = realDamage;

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