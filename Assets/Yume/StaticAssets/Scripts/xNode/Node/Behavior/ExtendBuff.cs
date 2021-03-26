namespace Yume.Battle {
  public class ExtendBuff : LeafNodeBase {
    public override bool Run() {
      string buffId = behaviorTree.blackboard.GetValue<string>(Blackboard.EXTEND_BUFF);
      if (BattleManager.buffManagers[owner].ContainBuff(buffId)) {
        int extendDuration = behaviorTree.blackboard.GetValue<int>(Blackboard.EXTEND_BUFF_DURATION);
        foreach (var buff in BattleManager.buffManagers[owner].buffDict[buffId]) {
          buff.duration += extendDuration;
        }
        return true;
      }
      return false;
    }
  }
}