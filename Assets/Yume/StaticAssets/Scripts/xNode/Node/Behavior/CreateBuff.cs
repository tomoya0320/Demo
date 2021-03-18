namespace Yume.Battle {
  public class CreateBuff : LeafNodeBase {
    public override bool Run() {
      Unit caster = behaviorTree.blackboard.GetValue<Unit>("caster");

      if (behaviorTree.blackboard.TryGetValue<BuffTemplate>("buff", out var buff)) {
        // 以caster和candidate为基准筛选出目标
        foreach (var target in buff.selector.Select(owner, caster)) {
          // 跳过已死亡的单位
          if (!target.isAlive) {
            continue;
          }
          // 因为blackboard是引用类型所以需要新建一个
          Blackboard blackboard = new Blackboard { { "caster", owner } };
          blackboard.Merge(buff.blackboard);
          BattleManager.buffManagers[target].AddBuff(buff.id, buff.stackType, blackboard, buff.behaviors);
        }
        return true;
      }
      return false;
    }
  }
}