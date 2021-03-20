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
    public CardData data { get; }
    public GameObject gameObject { get; }
    public UIBattleCard battleCard { get; }
    public Rect rect => rectTransform.rect;
    public CardTemplate cardTemplate { get; }
    public RectTransform rectTransform { get; }
    public CardAnimCallback cardAnimCallback { get; }
    public CardPhase cardPhase => cardTemplate.cardPhases[data.phase];

    public Card(Unit unit, CardArgs args, GameObject root, Func<Card, bool> onUseCard = null) {
      owner = unit;
      gameObject = root;
      rectTransform = gameObject.GetComponent<RectTransform>();

      data = new CardData(args.id, args.phase);
      cardTemplate = ResourceManager.Load<CardTemplate>(data.id);

      var objectContainer = gameObject.GetComponent<ObjectContainer>();
      objectContainer.GetComponent<Text>("NameText").text = cardPhase.name;
      if (data.phase == cardTemplate.cardPhases.Length - 1) {
        objectContainer.GetComponent<Text>("NameText").text += "[MAX]";
      }
      objectContainer.GetComponent<Text>("PhaseText").text = $"{data.phase + 1}";
      objectContainer.GetComponent<Text>("TypeText").text = cardTemplate.cardType.GetDescription();
      string energyConsumption = cardPhase.energyConsumption >= 0 ? cardPhase.energyConsumption.ToString() : "X";
      objectContainer.GetComponent<Text>("EnergyText").text = energyConsumption;
      objectContainer.GetComponent<Image>("IconImage").sprite = ResourceManager.Load<Sprite>(cardPhase.iconFileName);

      var cardAnimPrefab = ResourceManager.Load<GameObject>(cardPhase.cardAnimName);
      cardAnimCallback = Object.Instantiate(cardAnimPrefab, CardFactory.cardAnimFolder).GetComponent<CardAnimCallback>();
      cardAnimCallback.gameObject.SetActiveEx(false);
      cardAnimCallback.card = this;

      if (onUseCard == null) {
        gameObject.SetActiveEx(false);
      } else {
        battleCard = gameObject.AddComponent<UIBattleCard>().Init(this, () => onUseCard(this));
      }
      var descTextAutoScroll = new YTextAutoScroll(objectContainer.GetGameObject("Description"));
      descTextAutoScroll.SetTextContent(StringUtil.GetActualDescription(cardPhase.description, cardPhase.buffTemplates));
    }

    public IEnumerator Cast() {
      if (!candidate.isAlive) {
        yield break;
      }
      owner.playingCard = this;
      BattleManager.behaviorManagers[owner].Run(BehaviorTime.ON_BEFORE_PLAY_CARD);
      yield return cardAnimCallback.PlayAnim();
      BattleManager.behaviorManagers[owner].Run(BehaviorTime.ON_LATE_PLAY_CARD);
      owner.playingCard = null;
      candidate = null;
    }

    public void CleanUp() {
      Object.Destroy(gameObject);
      Object.Destroy(cardAnimCallback.gameObject);
    }
  }
}