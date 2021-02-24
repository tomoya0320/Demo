using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Yume.Data;
using Yume.Resource;
using Yume.UI;

namespace Yume.Battle {
  public class Card {
    public Unit owner;
    public bool active;
    public Rect rect { get; }
    public GameObject gameObject { get; }
    public int phase { private set; get; }
    public CardTemplate cardTemplate { get; }
    public CardPhase cardPhase => cardTemplate.cardPhases[phase];

    public Card(Unit unit, CardData card, GameObject root, Action<Card> onUseCard = null) {
      owner = unit;
      gameObject = root;
      phase = card.phase;
      cardTemplate = ResourceManager.Load<CardTemplate>(card.id);
      
      // 表现层
      var objectContainer = gameObject.GetComponent<ObjectContainer>();
      objectContainer.GetComponent<Text>("NameText").text = cardPhase.name;
      objectContainer.GetComponent<Text>("PhaseText").text = phase.ToString();
      objectContainer.GetComponent<Text>("TypeText").text = cardTemplate.cardType.GetDescription();
      objectContainer.GetComponent<Text>("EnergyText").text = cardPhase.energyConsumption.ToString();
      objectContainer.GetComponent<Image>("IconImage").sprite = ResourceManager.Load<Sprite>(cardPhase.iconFileName);
      if (onUseCard != null) {
        gameObject.AddComponent<UICardDrag>().Init(() => onUseCard(this), () => {
          if (gameObject.transform.position.y > 0) {
            if (cardPhase.energyConsumption > owner.GetAttrib(AttribType.ENERGY)) {
              MessageManager.AddMessage(new Message(Message.Style.CENTER, "能量不足！", null, 0.5f, false));
              return false;
            } else {
              return true;
            }
          } else {
            return false;
          }
        });
      }
      var descTextAutoScroll = new YTextAutoScroll(objectContainer.GetGameObject("Description"));
      descTextAutoScroll.SetTextContent(cardPhase.description);
      // TODO:测试用
      rect = gameObject.GetComponent<RectTransform>().rect;
      
      gameObject.SetActiveEx(false);
    }

    public bool Cast(Unit caster, Unit candidate) {
      // TODO:前置条件检测(比如能量消耗、其他特殊条件等)
      caster.AddAttrib(AttribType.ENERGY, -cardPhase.energyConsumption);
      
      foreach (var buffTemplate in cardPhase.buffTemplates) {
        // TODO:以caster和candidate为基准筛选出目标 测试用
        List<Unit> targets = buffTemplate.selector.Select(caster, candidate);
        foreach (var target in targets) {
#if BETA
          string content = $"[{caster.name}]对[{target.name}]使用了卡牌[{cardPhase.name}]";
          BattleManager.Log(content);
#endif
          // 因为blackboard是引用类型所以需要新建一个
          Blackboard blackboard = new Blackboard { { "caster", caster } };
          blackboard = blackboard.Merge(buffTemplate.blackboard);
          BattleManager.buffManagers[target].AddBuff(buffTemplate.id, buffTemplate.stackType, blackboard, buffTemplate.behaviors);
        }
      }
      
      return true;
    }
  }
}