namespace Yume.Battle {
  public class RemoveBuffOfBehaviorTree : LeafNodeBase {
    public override bool Run() {
      Unit owner = behaviorTree.unit;
      return BattleManager.buffManagers[owner].RemoveBuff(behaviorTree.buff);
    }
  }
}