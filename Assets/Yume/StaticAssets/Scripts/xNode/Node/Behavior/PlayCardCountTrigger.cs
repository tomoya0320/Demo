namespace Yume.Battle {
  public class PlayCardCountTrigger : LeafNodeBase {
    public override bool Run() {
      int totalPlayCardCount = owner.blackboard.GetValue<int>(Blackboard.PLAY_CARD_COUNT);
      int playCardCount = behaviorTree.blackboard.GetValue<int>(Blackboard.PLAY_CARD_COUNT);
      int playCardTriggerCount = behaviorTree.blackboard.GetValue<int>(Blackboard.PLAY_CARD_TRIGGER_COUNT);
      if (totalPlayCardCount - playCardCount >= playCardTriggerCount) {
        behaviorTree.blackboard[Blackboard.PLAY_CARD_COUNT] = totalPlayCardCount;
        return true;
      }
      return false;
    }
  }
}