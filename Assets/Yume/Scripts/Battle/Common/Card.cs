using UnityEngine;
using UnityEngine.UI;
using Yume.Data;
using Yume.Resource;
using Yume.UI;

namespace Yume.Battle {
  public class Card {
    private CardData m_card;
    public bool needDisplay => gameObject != null;
    public GameObject gameObject { private set; get; }

    public Card(CardData card, GameObject root = null) {
      m_card = card;
      gameObject = root;

      if (needDisplay) {
        var objectContainer = gameObject.GetComponent<ObjectContainer>();
        CardTemplate cardTemplate = ResourceManager.Load<CardTemplate>(m_card.id);
        CardPhase cardPhase = cardTemplate.cardPhases[m_card.phase];

        objectContainer.GetComponent<Text>("PhaseText").text = m_card.phase.ToString();
        objectContainer.GetComponent<Text>("TypeText").text = cardTemplate.cardType.GetDescription();
        objectContainer.GetComponent<Text>("NameText").text = cardPhase.name;
        objectContainer.GetComponent<Text>("EnergyText").text = cardPhase.energyConsumption.ToString();
        objectContainer.GetComponent<Image>("IconImage").sprite = ResourceManager.Load<Sprite>(cardPhase.iconFileName);
        var descTextAutoScroll = new YTextAutoScroll(objectContainer.GetGameObject("Description"));
        descTextAutoScroll.SetTextContent(StringUtil.GetActualDescription(cardPhase.description, cardPhase.blackboard));

        gameObject.SetActiveEx(false);
      }
    }
  }
}