using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Yume.UI;
using Object = UnityEngine.Object;

namespace Yume.Battle {
  public class Card {
    public Unit owner;
    public Unit candidate;
    public CardHeapType cardHeapType;
    
    public string id { get; }
    public int phase { get; }
    public GameObject gameObject { get; }
    public UIBattleCard battleCard { get; }
    public CardTemplate cardTemplate { get; }
    public RectTransform rectTransform { get; }
    public CardAnimCallback cardAnimCallback { get; }
    public CardPhase cardPhase => cardTemplate.cardPhases[phase];

    public Card(Unit unit, CardArgs args, GameObject root, Func<Card, bool> onUseCard = null) {
      id = args.id;
      phase = args.phase;
      owner = unit;
      gameObject = root;
      cardHeapType = CardHeapType.DRAW;
      rectTransform = gameObject.GetComponent<RectTransform>();

      cardTemplate = ResourceManager.Load<CardTemplate>(id);

      var objectContainer = gameObject.GetComponent<ObjectContainer>();
      objectContainer.GetComponent<Text>("NameText").text = cardPhase.name;
      if (phase == cardTemplate.cardPhases.Length - 1) {
        objectContainer.GetComponent<Text>("NameText").text += "[MAX]";
      }
      objectContainer.GetComponent<Text>("PhaseText").text = $"{phase + 1}";
      objectContainer.GetComponent<Text>("TypeText").text = cardTemplate.cardType.GetDescription();
      string energyConsumption = cardPhase.energyConsumption >= 0 ? cardPhase.energyConsumption.ToString() : "X";
      objectContainer.GetComponent<Text>("EnergyText").text = energyConsumption;
      objectContainer.GetComponent<Image>("IconImage").SetSprite(cardPhase.iconFileName);
      var descTextAutoScroll = new YTextAutoScroll(objectContainer.GetGameObject("Description"));
      descTextAutoScroll.SetTextContent(StringUtil.GetActualDescription(cardPhase.description, cardPhase.buffTemplates));

      cardAnimCallback = ObjectUtil.Instantiate<CardAnimCallback>(cardPhase.cardAnimName, CardFactory.cardAnimFolder);
      cardAnimCallback.gameObject.SetActiveEx(false);
      cardAnimCallback.SetCard(this);

      if (onUseCard != null) {
        battleCard = gameObject.AddComponent<UIBattleCard>().Init(this, () => onUseCard(this));
      }
    }

    public IEnumerator Cast() {
      if (candidate.isDead) {
        yield break;
      }
      
      owner.playingCard = this;
      BattleManager.behaviorManagers[owner].Run(BehaviorTime.ON_BEFORE_PLAY_CARD);
      yield return cardAnimCallback.PlayAnim();
      BattleManager.behaviorManagers[owner].Run(BehaviorTime.ON_LATE_PLAY_CARD);
      owner.playingCard = null;
      candidate = null;
    }

    public void OnUpdate() {
      battleCard?.OnUpdate();
    }

    public void CleanUp() {
      Object.Destroy(gameObject);
      Object.Destroy(cardAnimCallback.gameObject);
    }
  }
}