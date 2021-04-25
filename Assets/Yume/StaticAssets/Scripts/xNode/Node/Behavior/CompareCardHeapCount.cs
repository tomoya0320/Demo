using UnityEngine;

namespace Yume.Battle {
  public class CompareCardHeapCount : LeafNodeBase {
    public override bool Run() {
      if (owner is Character character) {
        var comparer = behaviorTree.blackboard.GetValue<CardHeapCountComparer>(Blackboard.CARD_HEAP_COUNT_COMPARER);
        int cardHeapCount = character.GetCards(comparer.cardHeapType).Count;
        return MathUtil.Compare(cardHeapCount, comparer.count, comparer.method);
      }
      Debug.LogError("比较牌堆数的节点仅限对角色使用！");
      return false;
    }
  }
}