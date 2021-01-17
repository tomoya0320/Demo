using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Yume.Battle;
using Yume.Broadcast;
using Yume.DB;
using Yume.Resource;
using Yume.String;
using EventType = Yume.Broadcast.EventType;

namespace Yume.UI {
  public class UICard : UIBase {
    private class UICardTerm : UIChildBase, IScrollTerm<CardDB> {
      private CardDB m_card;
      private Text m_nameText;
      private Text m_typeText;
      private Image m_iconImg;
      private Text m_phaseText;
      private Text m_energyText;
      private YTextAutoScroll m_descriptionText;
      public bool isValid => m_card != null;

      public UICardTerm(GameObject root) : base(root) {
        m_nameText = GetComponent<Text>("NameText");
        m_typeText = GetComponent<Text>("TypeText");
        m_iconImg = GetComponent<Image>("IconImage");
        m_phaseText = GetComponent<Text>("PhaseText");
        m_energyText = GetComponent<Text>("EnergyText");
        m_descriptionText = GetComponent<YTextAutoScroll>("DescriptionText");
      }

      public async void Update(CardDB data) {
        m_card = data;

        gameObject.SetActiveEx(m_card != null);

        if (m_card != null) {
          CardInfo cardInfo = DBManager.instance.GetCardInfo(m_card.id);
          m_nameText.text = cardInfo.name;
          m_phaseText.text = m_card.phase.ToString();
          m_typeText.text = cardInfo.cardType.GetDescription();
          m_energyText.text = cardInfo.cardPhases[m_card.phase].energyConsumption.ToString();
          m_iconImg.sprite = await ResourceManager.LoadAssetAsync<Sprite>(cardInfo.iconFileName);
          string description = cardInfo.cardPhases[m_card.phase].description;
          Blackboard blackboard = cardInfo.cardPhases[m_card.phase].blackboard;
          m_descriptionText.SetTextContent(StringUtil.GetActualDescription(description, blackboard));
        }
      }
    }

    private class UICardGroup : UIChildBase {
      private List<CardDB> m_cardList;
      private GridLayoutGroup m_cardGLG;
      private ScrollRect m_cardScrollRect;
      private UIScrollView<CardDB, UICardTerm> m_cardScrollView;
      private List<UICardTerm> m_cardTermList = new List<UICardTerm>();

      public UICardGroup(GameObject root) : base(root) {
        m_cardScrollRect = GetComponent<ScrollRect>("CardScrollRect");
        m_cardGLG = m_cardScrollRect.content.GetComponent<GridLayoutGroup>();
        
        m_cardList = DBManager.instance.userCards;

        for (int i = 0; i < m_cardScrollRect.content.childCount; i++) {
          m_cardTermList.Add(new UICardTerm(m_cardScrollRect.content.GetChild(i).gameObject));
        }

        m_cardScrollView = new UIScrollView<CardDB, UICardTerm>(m_cardScrollRect, m_cardGLG, m_cardList, m_cardTermList);

        EventCenter.AddListener(EventType.REFRESH_CARD_VIEW, m_cardScrollView.OnRefresh);
      }

      public void Destroy() {
        EventCenter.RemoveListener(EventType.REFRESH_CARD_VIEW, m_cardScrollView.OnRefresh);
      }
    }

    private UICardGroup m_cardGroup;

    public override void OnInit(UIData uiData, GameObject root) {
      base.OnInit(uiData, root);

      m_cardGroup = new UICardGroup(GetGameObject("CardGroup"));

      _AddListener();
    }

    private void _AddListener() {
      GetComponent<Button>("close").onClick.AddListener(() => UIManager.instance.CloseOne(name));
    }

    public override void Destroy() {
      base.Destroy();
      m_cardGroup.Destroy();
    }
  }
}