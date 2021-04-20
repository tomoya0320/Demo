namespace Yume.Battle {
  public class CreateBuff : LeafNodeBase {
    public override bool Run() {
      if (behaviorTree.blackboard.TryGetValue<BuffTemplates>(Blackboard.CREATE_BUFFS, out var buffTemplates)) {
        Unit caster = behaviorTree.blackboard.GetValue<Unit>(Blackboard.CASTER);
        foreach (var buffTemplate in buffTemplates) {
          int leftCount = 0;
          // 以caster和owner为基准筛选出目标
          foreach (var target in buffTemplate.selector.Select(caster, owner)) {
            // 跳过已死亡的单位
            if (target.isDead) {
              continue;
            }
            
            // 因为blackboard是引用类型所以需要新建一个
            Blackboard blackboard = new Blackboard { { Blackboard.CASTER, caster } };
            blackboard.Merge(buffTemplate.blackboard);
            BattleManager.buffManagers[target].AddBuff(
              buffTemplate.id, buffTemplate.stackType, blackboard, buffTemplate.behaviors, buffTemplate.display
            );
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