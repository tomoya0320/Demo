namespace Yume.Battle {
  public class ContainBuff : LeafNodeBase {
    public override bool Run() {
      string buffId = behaviorTree.blackboard.GetValue<string>(Blackboard.CONTAIN_BUFF);
      return BattleManager.buffManagers[owner].ContainBuff(buffId);
    }
  }
}