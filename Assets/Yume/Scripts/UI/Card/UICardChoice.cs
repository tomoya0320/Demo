using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Yume.Battle;
using Yume.DB;
using Yume.Resource;

namespace Yume.UI {
  public class UICardChoice : UIBase {
    private class UICardChoiceTerm : UIChildBase, IScrollTerm<CardDB> {
      private CardDB m_card;
      private Text m_nameText;
      private Text m_typeText;
      private Image m_iconImg;
      private Text m_phaseText;
      private Text m_energyText;
      private GameObject m_selected;
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
        GetComponent<Button>("SelectButton").onClick.AddListener(() => {
          if (m_cardChoiceGroup.selectedCards.Contains(m_card)) {
            m_cardChoiceGroup.selectedCards.Remove(m_card);
            m_cardChoiceGroup.UpdateView(false);
          } else if (m_cardChoiceGroup.selectedCards.Count < Constant.MAX_CARD_COUNT) {
            m_cardChoiceGroup.selectedCards.Add(m_card);
            m_cardChoiceGroup.UpdateView(false);
          }
        });
      }

      public async void UpdateScrollTerm(CardDB data) {
        m_card = data;

        gameObject.SetActiveEx(m_card != null);
        m_selected.SetActiveEx(m_cardChoiceGroup.selectedCards.Contains(m_card));

        if (m_card != null) {
          CardInfo cardInfo = DBManager.instance.GetCardInfo(m_card.id);
          CardPhase cardPhase = cardInfo.cardPhases[m_card.phase];

          m_phaseText.text = m_card.phase.ToString();
          m_typeText.text = cardInfo.cardType.GetDescription();
          m_nameText.text = cardPhase.name;
          m_energyText.text = cardPhase.energyConsumption.ToString();
          m_iconImg.sprite = await ResourceManager.LoadAssetAsync<Sprite>(cardPhase.iconFileName);
          m_descTextAutoScroll.SetTextContent(StringUtil.GetActualDescription(cardPhase.description, cardPhase.blackboard));
        }
      }
    }

    private class UICardChoiceGroup : UIChildBase {
      public List<CardDB> cardChoiceList;
      private ScrollRect m_cardChoiceScrollRect;
      public List<CardDB> selectedCards = new List<CardDB>();
      private UIScrollView<CardDB, UICardChoiceTerm> m_cardChoiceScrollView;
      private List<UICardChoiceTerm> m_cardChoiceTermList = new List<UICardChoiceTerm>();

      public UICardChoiceGroup(GameObject root) : base(root) {
        m_cardChoiceScrollRect = GetComponent<ScrollRect>("CardChoiceScrollRect");

        cardChoiceList = DBManager.instance.userCards;

        for (int i = 0; i < m_cardChoiceScrollRect.content.childCount; i++) {
          m_cardChoiceTermList.Add(new UICardChoiceTerm(m_cardChoiceScrollRect.content.GetChild(i).gameObject, this));
        }

        m_cardChoiceScrollView = new UIScrollView<CardDB, UICardChoiceTerm>(m_cardChoiceScrollRect, cardChoiceList, m_cardChoiceTermList);
      }

      public override void OnUpdate() {
        m_cardChoiceScrollView.OnUpdate();
      }

      public void UpdateView(bool recalLen) {
        m_cardChoiceScrollView.UpdateView(recalLen);
      }
    }

    private Action<List<string>> m_onSelected;
    private UICardChoiceGroup m_cardChoiceGroup;

    public override void OnInit(UIData uiData, GameObject root) {
      base.OnInit(uiData, root);

      m_cardChoiceGroup = new UICardChoiceGroup(GetGameObject("CardChoiceGroup"));

      _AddListener();
    }

    public override void Open(Action callback = null, params object[] args) {
      List<string> cardIds = args[0] as List<string>;
      m_cardChoiceGroup.selectedCards.Clear();
      foreach (var cardId in cardIds) {
        m_cardChoiceGroup.selectedCards.Add(DBManager.instance.GetCardDB(cardId));
      }
      m_onSelected = args[1] as Action<List<string>>;

      m_cardChoiceGroup.UpdateView(true);

      base.Open(callback, args);
    }

    public override void OnUpdate() {
      m_cardChoiceGroup.OnUpdate();
    }

    private void _AddListener() {
      GetComponent<Button>("ok").onClick.AddListener(() => {
        UIManager.instance.CloseOne(name);
        List<string> cardIds = new List<string>();
        foreach (var card in m_cardChoiceGroup.selectedCards) {
          cardIds.Add(card.id);
        }
        m_onSelected?.Invoke(cardIds);
      });
      GetComponent<Button>("cancel").onClick.AddListener(() => UIManager.instance.CloseOne(name));
    }
  }
}