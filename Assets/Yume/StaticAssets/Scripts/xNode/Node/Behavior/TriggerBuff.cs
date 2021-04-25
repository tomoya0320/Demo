namespace Yume.Battle {
  public class TriggerBuff : LeafNodeBase {
    public override bool Run() {
      string buffId = behaviorTree.blackboard.GetValue<string>(Blackboard.TRIGGER_BUFF);
      BattleManager.buffManagers[owner].TriggerBuff(buffId);
      return true;
    }
  }
}