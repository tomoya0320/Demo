namespace Yume.Battle {
  public class RemoveBuff : LeafNodeBase {
    public override bool Run() {
      var buffManager = BattleManager.buffManagers[owner];
      string buffId = behaviorTree.blackboard.GetValue<string>(Blackboard.REMOVE_BUFF);
      if (buffManager.ContainBuff(buffId)) {
        var buffs = buffManager.buffDict[buffId];
        // 避免迭代器失效
        for (int i = buffs.Count - 1; i >= 0; i--) {
          buffManager.RemoveBuff(buffs[i]);
        }
        return true;
      }
      return false;
    }
  }
}