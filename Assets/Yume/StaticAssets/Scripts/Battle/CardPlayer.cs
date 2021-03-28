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
        // 消耗当前所有能量值外的都属于extraEventCount
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
        var blackboard = card.owner.blackboard;
        var buffManager = BattleManager.buffManagers[card.candidate];
        if (buffManager.ContainBuff(_buffId)) {
          blackboard[$"{card.cardTemplate.id}:{Blackboard.EXTRA_EVENT_COUNT}"] = _extraEventCount;
        } else {
          blackboard[$"{card.cardTemplate.id}:{Blackboard.EXTRA_EVENT_COUNT}"] = 0;
        }
        return true;
      }
      return false;
    }
  }

  public class CardPlayerWithEnergyAndBuffStack : CardPlayerWithEnergy {
    [SerializeField]
    private string _buffId;

    public override bool TryPlay(Card card) {
      if (base.TryPlay(card)) {
        var blackboard = card.owner.blackboard;
        var buffManager = BattleManager.buffManagers[card.candidate];
        if (buffManager.ContainBuff(_buffId)) {
          blackboard[$"{card.cardTemplate.id}:{Blackboard.EXTRA_EVENT_COUNT}"] = buffManager.buffDict[_buffId].Count;
        } else {
          blackboard[$"{card.cardTemplate.id}:{Blackboard.EXTRA_EVENT_COUNT}"] = 0;
        }
        return true;
      }
      return false;
    }
  }
}