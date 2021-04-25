using UnityEngine;

namespace Yume.Battle {
  public class RandomRetainCard : LeafNodeBase {
    public override bool Run() {
      if (owner is Character character) {
        int retainCount = behaviorTree.blackboard.GetValue<int>(Blackboard.RETAIN_COUNT);
        // 排除本身就有保留属性的卡牌
        Card[] cards = MathUtil.FisherYatesShuffle(character.GetCards(CardHeapType.HAND).FindAll(card => !card.cardPhase.retain));
        for (int i = 0; i < cards.Length; i++) {
          if (i >= retainCount) {
            break;
          }
          character.retainCards.Add(cards[i]);
        }
        return true;
      }
      Debug.LogError("随机保留手牌的效果仅限对角色使用！");
      return false;
    }
  }
}