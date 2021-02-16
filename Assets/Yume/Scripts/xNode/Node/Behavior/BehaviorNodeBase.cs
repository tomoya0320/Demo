using UnityEngine;
using XNode;
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
      // 单位死亡
      if (!owner.isAlive) {
        owner.OnDie();
      }
      return true;
    }

    protected abstract float OnDamage(Unit caster);
  }
}