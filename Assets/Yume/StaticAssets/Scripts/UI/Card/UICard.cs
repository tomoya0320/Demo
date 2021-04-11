using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Yume.Broadcast;
using EventType = Yume.Broadcast.EventType;

namespace Yume.UI {
  public class UICard : UIBase {
    private class UICardTerm : UIChildBase, IScrollTerm<CardData> {
      private CardData m_card;
      private Text m_nameText;
      private Text m_typeText;
      private Image m_iconImg;
      private Text m_phaseText;
      private Text m_energyText;
      private CardTemplate m_cardTemplate;
      public bool isValid => m_card != null;
      private YTextAutoScroll m_descTextAutoScroll;

      public UICardTerm(GameObject root) : base(root) {
        m_nameText = GetComponent<Text>("NameText");
        m_typeText = GetComponent<Text>("TypeText");
        m_iconImg = GetComponent<Image>("IconImage");
        m_phaseText = GetComponent<Text>("PhaseText");
        m_energyText = GetComponent<Text>("EnergyText");
        m_descTextAutoScroll = new YTextAutoScroll(GetGameObject("Description"));

        _AddListener();
      }

      private void _AddListener() {
        GetComponent<Button>("button").onClick.AddListener(
          () => {
            if (m_card.phase < m_cardTemplate.cardPhases.Length - 1) {
              UIManager.OpenOne("card_update", null, m_card);
            }
          }
        );
      }

      public void UpdateScrollTerm(CardData card) {
        m_card = card;

        gameObject.SetActiveEx(m_card != null);

        if (m_card != null) {
          m_cardTemplate = ResourceManager.Load<CardTemplate>(m_card.id);
          CardPhase cardPhase = m_cardTemplate.cardPhases[m_card.phase];

          m_phaseText.text = $"{m_card.phase + 1}";
          m_typeText.text = m_cardTemplate.cardType.GetDescription();
          m_nameText.text = cardPhase.name;
          if (m_card.phase == m_cardTemplate.cardPhases.Length - 1) {
            m_nameText.text += "[MAX]";
          }
          if (cardPhase.energyConsumption >= 0) {
            m_energyText.text = cardPhase.energyConsumption.ToString();
          } else {
            m_energyText.text = "X";
          }
          m_iconImg.SetSpriteAsync(cardPhase.iconFileName);
          m_descTextAutoScroll.SetTextContent(StringUtil.GetActualDescription(cardPhase.description, cardPhase.buffTemplates));
        }
      }

      public IScrollTerm<CardData> Clone() {
        var clone = ObjectUtil.Instantiate(gameObject, gameObject.GetParent());
        return new UICardTerm(clone);
      }
    }

    private class UICardGroup : UIChildBase, IScrollGroup<CardData, UICardTerm> {
      private List<CardData> m_cardList;
      private ScrollRect m_cardScrollRect;
      public List<CardData> dataList => m_cardList;
      public List<UICardTerm> viewList => m_cardTermList;
      private UIScrollView<CardData, UICardTerm> m_cardScrollView;
      private List<UICardTerm> m_cardTermList = new List<UICardTerm>();

      public UICardGroup(GameObject root) : base(root) {
        m_cardScrollRect = GetComponent<ScrollRect>("CardScrollRect");
        
        m_cardList = DataManager.playerCards;
        
        for (int i = 0; i < m_cardScrollRect.content.childCount; i++) {
          m_cardTermList.Add(new UICardTerm(m_cardScrollRect.content.GetChild(i).gameObject));
        }
        m_cardScrollView = new UIScrollView<CardData, UICardTerm>(m_cardScrollRect, this);

        EventCenter.AddListener(EventType.UPDATE_CARD_INFO, _Refresh);
      }

      private void _Refresh() {
        m_cardScrollView.UpdateView();
      }

      public override void OnOpen() {
        m_cardList.Sort((left, right) => string.CompareOrdinal(left.id, right.id));
        m_cardScrollView.ResetNormalizedPos();
      }

      public override void OnUpdate() {
        m_cardScrollView.OnUpdate();
      }

      public override void Destroy() {
        EventCenter.RemoveListener(EventType.UPDATE_CARD_INFO, _Refresh);
      }
    }

    private UICardGroup m_cardGroup;

    public override void OnInit(UITemplate uiTemplate, GameObject gameObject) {
      base.OnInit(uiTemplate, gameObject);

      m_cardGroup = new UICardGroup(GetGameObject("CardGroup"));

      _AddListener();
    }

    public override void OnUpdate() {
      m_cardGroup.OnUpdate();
    }

    public override void Open(Action callback = null, params object[] args) {
      m_cardGroup.OnOpen();

      base.Open(callback, args);
    }

    private void _AddListener() {
      GetComponent<Button>("close").onClick.AddListener(() => UIManager.CloseOne(name));
    }

    public override void Destroy() {
      base.Destroy();
      m_cardGroup.Destroy();
    }
  }
}