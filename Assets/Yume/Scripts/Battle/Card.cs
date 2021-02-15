using System;
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

    public bool Cast(Unit caster, Unit target) {
      // TODO:前置条件检测(比如能量消耗、其他特殊条件等)
      caster.AddAttribValue(AttribType.ENERGY, -cardPhase.energyConsumption);
      
      if (!BattleManager.buffManagers.ContainsKey(target)) {
        Debug.LogError($"目标未正确初始化！name:{target.name}");
        return false;
      }
      if (string.IsNullOrEmpty(cardPhase.buffTemplateId)) {
        Debug.LogError("buffId为空！");
        return false;
      }
      
      // 因为blackboard是引用类型所以需要新建一个
      Blackboard blackboard = new Blackboard { { "caster", caster } };
      blackboard = blackboard.Merge(cardPhase.blackboard);
      BattleManager.buffManagers[target].AddBuff(cardPhase.buffTemplateId, blackboard);
      return true;
    }
  }
}