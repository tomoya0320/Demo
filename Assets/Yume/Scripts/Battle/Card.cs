using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Yume.Data;
using Yume.Resource;
using Yume.UI;
using Object = UnityEngine.Object;

namespace Yume.Battle {
  public class Card {
    public Unit owner;
    public Unit candidate;
    public Rect rect { get; }
    public CardData cardData { get; }
    public UICardDrag cardDrag { get; }
    public GameObject gameObject { get; }
    public CardTemplate cardTemplate { get; }
    public CardAnimCallback cardAnimCallback { get; }
    public CardPhase cardPhase => cardTemplate.cardPhases[cardData.phase];

    public Card(Unit unit, CardData data, GameObject root, Func<Card, bool> onUseCard = null) {
      owner = unit;
      gameObject = root;
      cardData = new CardData(data.id, data.phase);
      cardTemplate = ResourceManager.Load<CardTemplate>(cardData.id);
      
      var objectContainer = gameObject.GetComponent<ObjectContainer>();
      objectContainer.GetComponent<Text>("NameText").text = cardPhase.name;
      objectContainer.GetComponent<Text>("PhaseText").text = cardData.phase.ToString();
      objectContainer.GetComponent<Text>("TypeText").text = cardTemplate.cardType.GetDescription();
      string energyConsumption = cardPhase.energyConsumption >= 0 ? cardPhase.energyConsumption.ToString() : "X";
      objectContainer.GetComponent<Text>("EnergyText").text = energyConsumption;
      objectContainer.GetComponent<Image>("IconImage").sprite = ResourceManager.Load<Sprite>(cardPhase.iconFileName);

      var cardAnimPrefab = ResourceManager.Load<GameObject>(cardPhase.cardAnimName);
      cardAnimCallback = Object.Instantiate(cardAnimPrefab, CardFactory.cardAnimFolder).GetComponent<CardAnimCallback>();
      cardAnimCallback.gameObject.SetActiveEx(false);
      cardAnimCallback.card = this;
      
      if (onUseCard != null) {
        cardDrag = gameObject.AddComponent<UICardDrag>().Init(
          () => {
            if (gameObject.transform.position.y > 0) {
              candidate?.selected.SetActiveEx(false);
              if (cardTemplate.onlySelf) {
                candidate = owner;
              } else {
                candidate = BattleManager.GetNearestEnemy(gameObject.transform.position);
              }
              candidate?.selected.SetActiveEx(true);
            } else {
              candidate?.selected.SetActiveEx(false);
              candidate = null;
            }
          },
          () => onUseCard(this)
        );
      }
      var descTextAutoScroll = new YTextAutoScroll(objectContainer.GetGameObject("Description"));
      descTextAutoScroll.SetTextContent(cardPhase.description);
      
      // TODO:测试用
      rect = gameObject.GetComponent<RectTransform>().rect;
      
      gameObject.SetActiveEx(false);
    }

    public IEnumerator Cast() {
      if (!candidate.isAlive) {
        yield break;
      }
      
      yield return cardAnimCallback.PlayAnim();
      candidate = null;
    }

    public void CleanUp() {
      Object.Destroy(gameObject);
      Object.Destroy(cardAnimCallback.gameObject);
    }
  }
}