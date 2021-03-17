using UnityEngine;

namespace Yume.Battle {
  public abstract class CardPlayer {
    public abstract bool TryPlay(Card card);
  }

  public class CardPlayerWithEnergy : CardPlayer {
    public override bool TryPlay(Card card) {
      if (card.cardPhase.energyConsumption > card.owner.GetAttrib(AttribType.ENERGY)) {
        if (card.candidate != null) {
          card.candidate.SetSelectedEnable(false);
          card.candidate = null;
        }
        MessageManager.AddMessage(new Message(Message.Style.CENTER, "能量不足！", false, null, 0.5f));
        return false;
      }
      // 及时消耗能量
      if (card.cardPhase.energyConsumption > 0) {
        card.owner.AddAttrib(AttribType.ENERGY, -card.cardPhase.energyConsumption);
      } else if (card.cardPhase.energyConsumption < 0) {
        int energy = card.owner.GetAttrib(AttribType.ENERGY);
        card.owner.AddAttrib(AttribType.ENERGY, -energy);
        card.owner.blackboard[$"{card.cardTemplate.id}:eventCount"] = Mathf.RoundToInt(energy);
      }
      return true;
    }
  }
}