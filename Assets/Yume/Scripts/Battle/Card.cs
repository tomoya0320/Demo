using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Yume.Data;
using Yume.Resource;
using Yume.UI;

namespace Yume.Battle {
  public class Card {
    public bool active = true;
    public GameObject gameObject { get; }
    public CardPhase cardPhase { private set; get; }

    public Card(CardData card, GameObject root, Action<Card> onClickCard = null) {
      gameObject = root;

      // 表现层
      var objectContainer = gameObject.GetComponent<ObjectContainer>();
      CardTemplate cardTemplate = ResourceManager.Load<CardTemplate>(card.id);
      cardPhase = cardTemplate.cardPhases[card.phase];

      objectContainer.GetComponent<Text>("PhaseText").text = card.phase.ToString();
      objectContainer.GetComponent<Text>("TypeText").text = cardTemplate.cardType.GetDescription();
      objectContainer.GetComponent<Text>("NameText").text = cardPhase.name;
      objectContainer.GetComponent<Text>("EnergyText").text = cardPhase.energyConsumption.ToString();
      objectContainer.GetComponent<Image>("IconImage").sprite = ResourceManager.Load<Sprite>(cardPhase.iconFileName);
      if (onClickCard != null) {
        objectContainer.GetComponent<YButton>("Button").onClick.AddListener(() => onClickCard(this));
      }
      var descTextAutoScroll = new YTextAutoScroll(objectContainer.GetGameObject("Description"));
      descTextAutoScroll.SetTextContent(cardPhase.description);

      gameObject.SetActiveEx(false);
    }

    public bool Cast(Unit caster, Unit candidate) {
      // TODO:前置条件检测(比如能量消耗、其他特殊条件等)
      caster.AddAttrib(AttribType.ENERGY, -cardPhase.energyConsumption);
      
      foreach (var buffTemplate in cardPhase.buffTemplates) {
        // TODO:以caster和candidate为基准筛选出目标 测试用
        List<Unit> targets = new List<Unit> { candidate };

        foreach (var target in targets) {
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