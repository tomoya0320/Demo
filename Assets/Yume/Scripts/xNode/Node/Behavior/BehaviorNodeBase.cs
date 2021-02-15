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
    public override bool Run() {
      // 小心不要套娃了
      // 被造成伤害前的行为树
      BattleManager.behaviorManagers.TryGetValue(behaviorTree.unit, out var ownerBehaviorManager);
      ownerBehaviorManager?.Run(BehaviorTime.ON_BEFORE_DAMAGED);
      // 造成伤害前的行为树 
      BehaviorManager castBehaviorManager = null;
      Unit caster = behaviorTree.blackboard.GetValue<Unit>("caster");
      if (caster != null) {
        BattleManager.behaviorManagers.TryGetValue(caster, out castBehaviorManager);
      }
      castBehaviorManager?.Run(BehaviorTime.ON_BEFORE_DAMAGE);
      
      bool result = OnDamage();
      
      // 被造成伤害后的行为树
      ownerBehaviorManager?.Run(BehaviorTime.ON_LATE_DAMAGED);
      // 造成伤害后的行为树 
      castBehaviorManager?.Run(BehaviorTime.ON_LATE_DAMAGE);
      // 单位死亡
      if (!behaviorTree.unit.isAlive) {
        behaviorTree.unit.OnDie();
      }
      return result;
    }

    protected abstract bool OnDamage();
  }
}