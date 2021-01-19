using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Yume.Battle;
using Yume.DB;
using Yume.Resource;
using Yume.String;

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
      private UICardChoice m_cardChoice;
      public bool isValid => m_card != null;
      private YTextAutoScroll m_descTextAutoScroll;

      public UICardChoiceTerm(GameObject root, UICardChoice cardChoice) : base(root) {
        m_selected = GetGameObject("Selected");
        m_nameText = GetComponent<Text>("NameText");
        m_typeText = GetComponent<Text>("TypeText");
        m_iconImg = GetComponent<Image>("IconImage");
        m_phaseText = GetComponent<Text>("PhaseText");
        m_energyText = GetComponent<Text>("EnergyText");
        m_descTextAutoScroll = new YTextAutoScroll(GetGameObject("Description"));

        m_cardChoice = cardChoice;

        _AddListener();
      }

      private void _AddListener() {
        GetComponent<Button>("SelectButton").onClick.AddListener(() => {
          if (m_cardChoice.m_selectedCards.Contains(m_card)) {
            m_cardChoice.m_selectedCards.Remove(m_card);
          } else if (m_cardChoice.m_selectedCards.Count < m_cardChoice.m_maxCardNum) {
            m_cardChoice.m_selectedCards.Add(m_card);
          }
          m_cardChoice.m_cardChoiceGroup.OnRefresh();
        });
      }

      public async void Update(CardDB data) {
        m_card = data;

        gameObject.SetActiveEx(m_card != null);
        m_selected.SetActiveEx(m_cardChoice.m_selectedCards.Contains(m_card));

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
      private List<CardDB> m_cardChoiceList;
      private GridLayoutGroup m_cardChoiceGLG;
      private ScrollRect m_cardChoiceScrollRect;
      private UIScrollView<CardDB, UICardChoiceTerm> m_cardChoiceScrollView;
      private List<UICardChoiceTerm> m_cardChoiceTermList = new List<UICardChoiceTerm>();

      public UICardChoiceGroup(GameObject root, UICardChoice cardChoice) : base(root) {
        m_cardChoiceScrollRect = GetComponent<ScrollRect>("CardChoiceScrollRect");
        m_cardChoiceGLG = m_cardChoiceScrollRect.content.GetComponent<GridLayoutGroup>();

        m_cardChoiceList = DBManager.instance.userCards;

        for (int i = 0; i < m_cardChoiceScrollRect.content.childCount; i++) {
          m_cardChoiceTermList.Add(new UICardChoiceTerm(m_cardChoiceScrollRect.content.GetChild(i).gameObject, cardChoice));
        }

        m_cardChoiceScrollView = new UIScrollView<CardDB, UICardChoiceTerm>(m_cardChoiceScrollRect, m_cardChoiceGLG, m_cardChoiceList, m_cardChoiceTermList);
      }

      public void OnRefresh() {
        m_cardChoiceScrollView.OnRefresh();
      }
    }

    private int m_maxCardNum;
    private Action<List<string>> m_onSelected;
    private UICardChoiceGroup m_cardChoiceGroup;
    private List<CardDB> m_selectedCards = new List<CardDB>();

    public override void OnInit(UIData uiData, GameObject root) {
      base.OnInit(uiData, root);

      m_cardChoiceGroup = new UICardChoiceGroup(GetGameObject("CardChoiceGroup"), this);

      _AddListener();
    }

    public override void Open(Action callback = null, params object[] args) {
      m_maxCardNum = (int)args[0];
      List<string> cardIds = args[1] as List<string>;
      m_selectedCards.Clear();
      foreach (var cardId in cardIds) {
        m_selectedCards.Add(DBManager.instance.GetCardDB(cardId));
      }
      m_onSelected = args[2] as Action<List<string>>;

      m_cardChoiceGroup.OnRefresh();

      base.Open(callback, args);
    }

    private void _AddListener() {
      GetComponent<Button>("ok").onClick.AddListener(() => {
        UIManager.instance.CloseOne(name);
        List<string> cardIds = new List<string>();
        foreach (var card in m_selectedCards) {
          cardIds.Add(card.id);
        }
        m_onSelected?.Invoke(cardIds);
      });
      GetComponent<Button>("cancel").onClick.AddListener(() => UIManager.instance.CloseOne(name));
    }
  }
}