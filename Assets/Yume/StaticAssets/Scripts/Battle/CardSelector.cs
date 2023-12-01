using System.Collections.Generic;

namespace Yume.Battle {
  public abstract class CardSelector {
    public CardHeapType from;
    public CardHeapType to;

    public abstract List<Card> Select(List<Card> cards);
  }

  public class RandomSelector : CardSelector {
    public int selectCount;

    public override List<Card> Select(List<Card> cards) {
      List<Card> result = new List<Card>(MathUtil.FisherYatesShuffle(cards));
      if (selectCount > 0) {
        int removeCount = result.Count - selectCount;
        while (removeCount > 0) {
          result.RemoveAt(0);
          removeCount--;
        }
      }
      return result;
    }
  }

  public class RandomSelectorExceptById : RandomSelector {
    public string exceptCardId;

    public override List<Card> Select(List<Card> cards) {
      cards = cards.FindAll(card => card.id != exceptCardId);
      return base.Select(cards);
    }
  }
}