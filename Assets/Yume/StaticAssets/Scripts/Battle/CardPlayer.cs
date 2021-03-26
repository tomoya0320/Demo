using UnityEngine;

namespace Yume.Battle {
  public abstract class CardPlayer {
    public abstract bool TryPlay(Card card);
  }

  public class CardPlayerWithEnergy : CardPlayer {
    public override bool TryPlay(Card card) {
      if (card.cardPhase.energyConsumption > card.owner.GetAttrib(AttribType.ENERGY)) {
        MessageManager.AddMessage(new Message(Message.Style.CENTER, "能量不足！", false, null, 0.5f));
        return false;
      }
      // 及时消耗能量
      if (card.cardPhase.energyConsumption > 0) {
        card.owner.AddAttrib(AttribType.ENERGY, -card.cardPhase.energyConsumption);
      } else if (card.cardPhase.energyConsumption < 0) {
        int energy = card.owner.GetAttrib(AttribType.ENERGY);
        card.owner.AddAttrib(AttribType.ENERGY, -energy);
        card.owner.blackboard[$"{card.cardTemplate.id}:{Blackboard.EVENT_COUNT}"] = energy;
      }
      
      return true;
    }
  }

  public class CardPlayerWithEnergyAndBuff : CardPlayerWithEnergy {
    [SerializeField]
    private string _buffId;
    [SerializeField]
    private int _extraEventCount;

    public override bool TryPlay(Card card) {
      if (base.TryPlay(card)) {
        if (BattleManager.buffManagers[card.candidate].ContainBuff(_buffId)) {
          card.owner.blackboard[$"{card.cardTemplate.id}:{Blackboard.EXTRA_EVENT_COUNT}"] = _extraEventCount;
        } else {
          card.owner.blackboard[$"{card.cardTemplate.id}:{Blackboard.EXTRA_EVENT_COUNT}"] = 0;
        }
        return true;
      }
      return false;
    }
  }
}