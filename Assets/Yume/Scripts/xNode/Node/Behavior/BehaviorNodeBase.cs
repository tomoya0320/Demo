using UnityEngine;
using XNode;
using Yume.Audio;
using Yume.Data;

namespace Yume.Battle {
  public abstract class BehaviorNodeBase : Node {
    protected BehaviorTree behaviorTree;

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

  public abstract class IntermediateNodeBase : BehaviorNodeBase {
    [Input(ShowBackingValue.Never, ConnectionType.Override)]
    public int last;
    [Output(ShowBackingValue.Never)]
    public int next;
  }

  public abstract class DamageBase : LeafNodeBase {
    public sealed override bool Run() {
      // 小心不要套娃了
      // 检测发起者是否为空
      Unit caster = behaviorTree.blackboard.GetValue<Unit>("caster");
      if (caster == null) {
        Debug.LogError("攻击发起者为空！");
        return false;
      }
      // 被造成伤害前的行为树
      Unit owner = behaviorTree.unit;
      BattleManager.behaviorManagers.TryGetValue(owner, out var ownerBehaviorManager);
      ownerBehaviorManager?.Run(BehaviorTime.ON_BEFORE_DAMAGED);
      // 造成伤害前的行为树 
      BattleManager.behaviorManagers.TryGetValue(caster, out BehaviorManager castBehaviorManager);
      castBehaviorManager?.Run(BehaviorTime.ON_BEFORE_DAMAGE);

      float realDamage = OnDamage(caster);
      // 播放受击音效
      string hurtSound = behaviorTree.blackboard.GetValue("hurtSound", StringMap.DEFAULT_HURT_SOUND);
      AudioManager.PlaySound(hurtSound);
      // 显示数字
      NumFactory.ShowNum(-realDamage, owner.gameObject.transform.position, Color.red);

      // 保存最后一次造成的伤害值
      if (caster != null) {
        caster.blackboard["lastDamageValue"] = realDamage;
      }
      // 保存最后一次被造成的伤害值
      owner.blackboard["lastDamagedValue"] = realDamage;

      // 被造成伤害后的行为树
      ownerBehaviorManager?.Run(BehaviorTime.ON_LATE_DAMAGED);
      // 造成伤害后的行为树 
      castBehaviorManager?.Run(BehaviorTime.ON_LATE_DAMAGE);

      if (owner.isAlive) {
        // 受伤
        owner.animator.SetTrigger("hurt");
      } else {
        BattleManager.behaviorManagers[owner].Run(BehaviorTime.ON_BEFORE_DIE);
      }

      // 在即将死亡前执行行为树有可能会脱离死亡状态
      if (!owner.isAlive) {
        // 单位死亡
        owner.OnDie();
      }
      
      return true;
    }

    protected abstract float OnDamage(Unit caster);
  }
}