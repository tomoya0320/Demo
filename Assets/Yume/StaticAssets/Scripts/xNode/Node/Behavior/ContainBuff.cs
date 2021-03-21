namespace Yume.Battle {
  public class ContainBuff : LeafNodeBase {
    public override bool Run() {
      if (behaviorTree.blackboard.ContainsKey<string>(Blackboard.CONTAIN_BUFF)) {
        string buffId = behaviorTree.blackboard.GetValue<string>(Blackboard.CONTAIN_BUFF);
        return BattleManager.buffManagers[owner].ContainBuff(buffId);
      }
      return false;
    }
  }
}