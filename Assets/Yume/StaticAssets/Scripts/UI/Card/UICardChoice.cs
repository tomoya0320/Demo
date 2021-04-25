using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Yume.Broadcast;
using EventType = Yume.Broadcast.EventType;

namespace Yume.UI {
  public class UICardChoice : UIBase {
    private class UICardChoiceTerm : UIChildBase, IScrollTerm<CardData> {
      private CardData m_card;
      private Text m_nameText;
      private Text m_typeText;
      private Image m_iconImg;
      private Text m_phaseText;
      private Text m_energyText;
      private GameObject m_selected;
      public CardData card => m_card;
      private CardTemplate m_cardTemplate;
      public bool isValid => m_card != null;
      private UICardChoiceContainer m_cardChoiceContainer;
      private YTextAutoScroll m_descTextAutoScroll;

      public UICardChoiceTerm(GameObject root, UICardChoiceContainer cardChoiceContainer) : base(root) {
        m_selected = GetGameObject("Selected");
        m_nameText = GetComponent<Text>("NameText");
        m_typeText = GetComponent<Text>("TypeText");
        m_iconImg = GetComponent<Image>("IconImage");
        m_phaseText = GetComponent<Text>("PhaseText");
        m_energyText = GetComponent<Text>("EnergyText");
        m_descTextAutoScroll = new YTextAutoScroll(GetGameObject("Description"));

        m_cardChoiceContainer = cardChoiceContainer;

        _AddListener();
      }

      private void _AddListener() {
        var button = GetComponent<YButton>("SelectButton");
        button.onClick.AddListener(
          () => {
            if (m_cardChoiceContainer.selectedCards.Contains(m_card)) {
              m_cardChoiceContainer.selectedCards.Remove(m_card);
              m_cardChoiceContainer.UpdateView(false);
            } else if (m_cardChoiceContainer.selectedCards.Count < Constant.MAX_CARD_COUNT) {
              m_cardChoiceContainer.selectedCards.Add(m_card);
              m_cardChoiceContainer.UpdateView(false);
            }
          }
        );
        button.onLongPress += () => {
          if (m_card.phase < m_cardTemplate.cardPhases.Length - 1) {
            UIManager.OpenOne("card_update", null, m_card);
          }
        };
      }

      public void UpdateScrollTerm(CardData card) {
        m_card = card;

        gameObject.SetActiveEx(m_card != null);
        m_selected.SetActiveEx(m_cardChoiceContainer.selectedCards.Contains(m_card));

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
        return new UICardChoiceTerm(clone, m_cardChoiceContainer);
      }
    }

    private class UICardChoiceContainer : UIChildBase, IScrollContainer<CardData, UICardChoiceTerm> {
      public List<CardData> cardChoiceList;
      private ScrollRect m_cardChoiceScrollRect;
      public List<CardData> dataList => cardChoiceList;
      public List<CardData> selectedCards = new List<CardData>();
      public List<UICardChoiceTerm> viewList => m_cardChoiceTermList;
      private UIScrollView<CardData, UICardChoiceTerm> m_cardChoiceScrollView;
      private List<UICardChoiceTerm> m_cardChoiceTermList = new List<UICardChoiceTerm>();
      public UIScrollView<CardData, UICardChoiceTerm> cardChoiceScrollView => m_cardChoiceScrollView;

      public UICardChoiceContainer(GameObject root) : base(root) {
        m_cardChoiceScrollRect = GetComponent<ScrollRect>("CardChoiceScrollRect");
        
        cardChoiceList = DataManager.playerCards;
        
        for (int i = 0; i < m_cardChoiceScrollRect.content.childCount; i++) {
          m_cardChoiceTermList.Add(new UICardChoiceTerm(m_cardChoiceScrollRect.content.GetChild(i).gameObject, this));
        }

        m_cardChoiceScrollView = new UIScrollView<CardData, UICardChoiceTerm>(m_cardChoiceScrollRect, this);
        
        EventCenter.AddListener(EventType.UPDATE_CARD_INFO, _Refresh);
      }

      private void _Refresh() {
        m_cardChoiceScrollView.UpdateView();
      }
      
      public override void OnUpdate() {
        m_cardChoiceScrollView.OnUpdate();
      }

      public void UpdateView(bool recalLen) {
        m_cardChoiceScrollView.UpdateView(recalLen);
      }

      public override void OnOpen() {
        cardChoiceList.Sort((left, right) => string.CompareOrdinal(left.id, right.id));
        m_cardChoiceScrollView.ResetNormalizedPos();
      }

      public override void Destroy() {
        EventCenter.RemoveListener(EventType.UPDATE_CARD_INFO, _Refresh);
      }
    }

    private Action<List<int>> m_onSelected;
    private UICardChoiceContainer m_cardChoiceContainer;

    public override void OnInit(UITemplate uiTemplate, GameObject gameObject) {
      base.OnInit(uiTemplate, gameObject);

      m_cardChoiceContainer = new UICardChoiceContainer(GetGameObject("CardChoiceContainer"));

      _AddListener();
    }

    public override void Open(Action callback = null, params object[] args) {
      List<int> cardGuids = args[0] as List<int>;
      m_cardChoiceContainer.selectedCards.Clear();
      foreach (var cardGuid in cardGuids) {
        m_cardChoiceContainer.selectedCards.Add(DataManager.GetCard(cardGuid));
      }
      m_onSelected = args[1] as Action<List<int>>;
      
      m_cardChoiceContainer.OnOpen();

      base.Open(callback, args);
    }
    
    public override RectTransform GetDynamicListObj(string list, string id) {
      if (m_cardChoiceContainer.gameObject.name == list) {
        int index = m_cardChoiceContainer.dataList.FindLastIndex(data => data.id == id);
        if (index >= 0) {
          m_cardChoiceContainer.cardChoiceScrollView.SetAsFirstDisplay(index);
          var view = m_cardChoiceContainer.viewList.FindLast(v => v.card?.id == id);
          return view.gameObject.GetComponent<RectTransform>();
        }
      }
      return null;
    }

    public override void OnUpdate() {
      m_cardChoiceContainer.OnUpdate();
    }

    private void _AddListener() {
      GetComponent<Button>("ok").onClick.AddListener(() => {
        UIManager.CloseOne(name);
        List<int> cardGuids = new List<int>();
        foreach (var card in m_cardChoiceContainer.selectedCards) {
          cardGuids.Add(card.guid);
        }
        cardGuids.Sort();
        m_onSelected?.Invoke(cardGuids);
      });
      GetComponent<Button>("cancel").onClick.AddListener(() => UIManager.CloseOne(name));
    }

    public override void Destroy() {
      base.Destroy();
      m_cardChoiceContainer.Destroy();
    }
  }
}