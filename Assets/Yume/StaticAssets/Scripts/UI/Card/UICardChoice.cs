using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

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
      private CardTemplate m_cardTemplate;
      public bool isValid => m_card != null;
      private UICardChoiceGroup m_cardChoiceGroup;
      private YTextAutoScroll m_descTextAutoScroll;

      public UICardChoiceTerm(GameObject root, UICardChoiceGroup cardChoiceGroup) : base(root) {
        m_selected = GetGameObject("Selected");
        m_nameText = GetComponent<Text>("NameText");
        m_typeText = GetComponent<Text>("TypeText");
        m_iconImg = GetComponent<Image>("IconImage");
        m_phaseText = GetComponent<Text>("PhaseText");
        m_energyText = GetComponent<Text>("EnergyText");
        m_descTextAutoScroll = new YTextAutoScroll(GetGameObject("Description"));

        m_cardChoiceGroup = cardChoiceGroup;

        _AddListener();
      }

      private void _AddListener() {
        var button = GetComponent<YButton>("SelectButton");
        button.onClick.AddListener(
          () => {
            if (m_cardChoiceGroup.selectedCards.Contains(m_card)) {
              m_cardChoiceGroup.selectedCards.Remove(m_card);
              m_cardChoiceGroup.UpdateView(false);
            } else if (m_cardChoiceGroup.selectedCards.Count < Constant.MAX_CARD_COUNT) {
              m_cardChoiceGroup.selectedCards.Add(m_card);
              m_cardChoiceGroup.UpdateView(false);
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
        m_selected.SetActiveEx(m_cardChoiceGroup.selectedCards.Contains(m_card));

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
          m_iconImg.sprite = ResourceManager.Load<Sprite>(cardPhase.iconFileName);
          m_descTextAutoScroll.SetTextContent(StringUtil.GetActualDescription(cardPhase.description, cardPhase.buffTemplates));
        }
      }

      public IScrollTerm<CardData> Clone() {
        var clone = Object.Instantiate(gameObject, gameObject.GetParent());
        return new UICardChoiceTerm(clone, m_cardChoiceGroup);
      }
    }

    private class UICardChoiceGroup : UIChildBase, IScrollGroup<CardData, UICardChoiceTerm> {
      public List<CardData> cardChoiceList;
      private ScrollRect m_cardChoiceScrollRect;
      public List<CardData> dataList => cardChoiceList;
      public List<CardData> selectedCards = new List<CardData>();
      public List<UICardChoiceTerm> viewList => m_cardChoiceTermList;
      private UIScrollView<CardData, UICardChoiceTerm> m_cardChoiceScrollView;
      private List<UICardChoiceTerm> m_cardChoiceTermList = new List<UICardChoiceTerm>();

      public UICardChoiceGroup(GameObject root) : base(root) {
        m_cardChoiceScrollRect = GetComponent<ScrollRect>("CardChoiceScrollRect");

        cardChoiceList = DataManager.playerCards;

        for (int i = 0; i < m_cardChoiceScrollRect.content.childCount; i++) {
          m_cardChoiceTermList.Add(new UICardChoiceTerm(m_cardChoiceScrollRect.content.GetChild(i).gameObject, this));
        }

        m_cardChoiceScrollView = new UIScrollView<CardData, UICardChoiceTerm>(m_cardChoiceScrollRect, this);
      }

      public override void OnUpdate() {
        m_cardChoiceScrollView.OnUpdate();
      }

      public void UpdateView(bool recalLen) {
        m_cardChoiceScrollView.UpdateView(recalLen);
      }
    }

    private Action<List<int>> m_onSelected;
    private UICardChoiceGroup m_cardChoiceGroup;

    public override void OnInit(UITemplate uiTemplate, GameObject gameObject) {
      base.OnInit(uiTemplate, gameObject);

      m_cardChoiceGroup = new UICardChoiceGroup(GetGameObject("CardChoiceGroup"));

      _AddListener();
    }

    public override void Open(Action callback = null, params object[] args) {
      List<int> cardGuids = args[0] as List<int>;
      m_cardChoiceGroup.selectedCards.Clear();
      foreach (var cardGuid in cardGuids) {
        m_cardChoiceGroup.selectedCards.Add(DataManager.GetCard(cardGuid));
      }
      m_onSelected = args[1] as Action<List<int>>;

      m_cardChoiceGroup.UpdateView(true);

      base.Open(callback, args);
    }

    public override void OnUpdate() {
      m_cardChoiceGroup.OnUpdate();
    }

    private void _AddListener() {
      GetComponent<Button>("ok").onClick.AddListener(() => {
        UIManager.CloseOne(name);
        List<int> cardGuids = new List<int>();
        foreach (var card in m_cardChoiceGroup.selectedCards) {
          cardGuids.Add(card.guid);
        }
        cardGuids.Sort();
        m_onSelected?.Invoke(cardGuids);
      });
      GetComponent<Button>("cancel").onClick.AddListener(() => UIManager.CloseOne(name));
    }
  }
}