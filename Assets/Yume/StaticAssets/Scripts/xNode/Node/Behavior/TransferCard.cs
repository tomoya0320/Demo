using UnityEngine;

namespace Yume.Battle {
  public class TransferCard : LeafNodeBase {
    public override bool Run() {
      if (owner is Character character) {
        CardSelector selector = behaviorTree.blackboard.GetValue<CardSelector>(Blackboard.CARD_SELECTOR);
        character.TransferCards(selector);
        return true;
      }
      Debug.LogError("移动卡牌位置的效果仅限对角色使用！");
      return false;
    }
  }
}