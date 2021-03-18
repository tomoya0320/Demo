namespace Yume.Battle {
  public class RemoveBuffOfBehaviorTree : LeafNodeBase {
    public override bool Run() {
      return BattleManager.buffManagers[owner].RemoveBuff(behaviorTree.buff);
    }
  }
}