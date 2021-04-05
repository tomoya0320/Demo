using UnityEngine;

namespace Yume.Battle {
  public class DiscardAndDrawCard : LeafNodeBase {
    public override bool Run() {
      if (owner is Character character) {
        int count = behaviorTree.blackboard.GetValue<int>(Blackboard.DISCARD_AND_DRAW_COUNT);
        // 实际操作的卡牌数
        if (count <= 0 || count > character.GetCards(CardHeapType.HAND).Count) {
          count = character.GetCards(CardHeapType.HAND).Count;
        }
        character.TransferCards(
          new RandomSelector {
            from = CardHeapType.HAND,
            to = CardHeapType.DISCARD,
            selectCount = count,
          }
        );
        if (character.GetCards(CardHeapType.DRAW).Count < count) {
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