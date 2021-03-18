using System.Collections.Generic;
using UnityEngine;

namespace Yume.Battle {
  public abstract class CardSelector {
    public CardHeapType from;
    public CardHeapType to;

    public abstract List<Card> Select(List<Card> cards);
  }

  public class RandomSelector : CardSelector {
    [SerializeField]
    private int _selectCount;

    public override List<Card> Select(List<Card> cards) {
      List<Card> result = new List<Card>(MathUtil.FisherYatesShuffle(cards.ToArray()));
      int removeCount = result.Count - _selectCount;
      while (removeCount > 0) {
        result.RemoveAt(0);
        removeCount--;
      }
      return result;
    }
  }
}