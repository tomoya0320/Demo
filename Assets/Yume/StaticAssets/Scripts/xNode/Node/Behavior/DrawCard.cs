using UnityEngine;

namespace Yume.Battle {
  public class DrawCard : LeafNodeBase {
    public override bool Run() {
      if (owner is Character character) {
        int count = behaviorTree.blackboard.GetValue<int>(Blackboard.DRAW_COUNT);
        if (count > character.GetCards(CardHeapType.DRAW).Count) {
          character.TransferCards(
            new RandomSelector {
              from = CardHeapType.DISCARD,
              to = CardHeapType.DRAW,
              selectCount = character.GetCards(CardHeapType.DISCARD).Count,
            }
          );
        }
        character.TransferCards(
          new RandomSelector {
            from = CardHeapType.DRAW,
            to = CardHeapType.HAND,
            selectCount = count,
          }
        );
        return true;
      }
      Debug.LogError("弃牌抽牌的效果仅限对角色使用！");
      return false;
    }
  }
}