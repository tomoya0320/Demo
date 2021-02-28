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
    public UICardDrag cardDrag { get; }
    public GameObject gameObject { get; }
    public int phase { private set; get; }
    public CardTemplate cardTemplate { get; }
    public CardAnimCallback cardAnimCallback { get; }
    public CardPhase cardPhase => cardTemplate.cardPhases[phase];

    public Card(Unit unit, CardData card, GameObject root, Func<Card, bool> onUseCard = null) {
      owner = unit;
      gameObject = root;
      phase = card.phase;
      cardTemplate = ResourceManager.Load<CardTemplate>(card.id);
      
      var objectContainer = gameObject.GetComponent<ObjectContainer>();
      objectContainer.GetComponent<Text>("NameText").text = cardPhase.name;
      objectContainer.GetComponent<Text>("PhaseText").text = phase.ToString();
      objectContainer.GetComponent<Text>("TypeText").text = cardTemplate.cardType.GetDescription();
      objectContainer.GetComponent<Text>("EnergyText").text = cardPhase.energyConsumption.ToString();
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
              candidate = BattleManager.GetNearestEnemy(gameObject.transform.position);
              candidate?.selected.SetActiveEx(true);
            } else {
              candidate?.selected.SetActiveEx(false);
              candidate = null;
            }
          },
          () => onUseCard(this), 
          () => {
            if (cardPhase.energyConsumption > owner.GetAttrib(AttribType.ENERGY)) {
              MessageManager.AddMessage(new Message(Message.Style.CENTER, "能量不足！", null, 0.5f, false));
              return false;
            } else {
              return true;
            }
          }
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
      
      // TODO:前置条件需求消耗(比如能量消耗等)
      owner.AddAttrib(AttribType.ENERGY, -cardPhase.energyConsumption);
      yield return cardAnimCallback.PlayAnim();
      candidate = null;
    }

    public void CleanUp() {
      Object.Destroy(gameObject);
      Object.Destroy(cardAnimCallback.gameObject);
    }
  }
}