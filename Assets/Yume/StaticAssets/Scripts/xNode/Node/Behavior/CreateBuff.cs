namespace Yume.Battle {
  public class CreateBuff : LeafNodeBase {
    public override bool Run() {
      if (behaviorTree.blackboard.TryGetValue<BuffTemplates>(Blackboard.CREATE_BUFFS, out var buffTemplates)) {
        Unit caster = behaviorTree.blackboard.GetValue<Unit>(Blackboard.CASTER);
        foreach (var buffTemplate in buffTemplates) {
          int leftCount = 0;
          // 以owner和caster为基准筛选出目标
          foreach (var target in buffTemplate.selector.Select(owner, caster)) {
            // 跳过已死亡的单位
            if (!target.isAlive) {
              continue;
            }
            // 因为blackboard是引用类型所以需要新建一个
            Blackboard blackboard = new Blackboard { { Blackboard.CASTER, owner } };
            blackboard.Merge(buffTemplate.blackboard);
            BattleManager.buffManagers[target].AddBuff(buffTemplate.id, buffTemplate.stackType, blackboard, buffTemplate.behaviors);
            if (target.isAlive) {
              leftCount++;
            }
          }
          if (leftCount == 0) {
            return false;
          }
        }
        return true;
      }
      return false;
    }
  }
}