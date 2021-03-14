using System;
using UnityEngine;
using UnityEngine.UI;

namespace Yume.UI {
  public class UICardUpdate : UIBase {
    private Text m_oldNameText;
    private Text m_oldTypeText;
    private Image m_oldIconImg;
    private Text m_oldPhaseText;
    private Text m_oldEnergyText;
    private YTextAutoScroll m_oldDescTextAutoScroll; 
    
    private Text m_newNameText;
    private Text m_newTypeText;
    private Image m_newIconImg;
    private Text m_newPhaseText;
    private Text m_newEnergyText;
    private YTextAutoScroll m_newDescTextAutoScroll;

    private CardData m_card;
    private Button m_updateBtn;
    private Transform m_requestItems;
    private CardTemplate m_cardTemplate;
    private CardPhase cardPhase => m_cardTemplate.cardPhases[m_card.phase];
    private bool isMaxCardPhase => m_card.phase == m_cardTemplate.cardPhases.Length - 1;
    private CardPhase nextCardPhase => isMaxCardPhase ? cardPhase : m_cardTemplate.cardPhases[m_card.phase + 1];
    
    public override void OnInit(UITemplate uiTemplate, GameObject gameObject) {
      base.OnInit(uiTemplate, gameObject);

      m_updateBtn = GetComponent<Button>("ok");
      m_requestItems = GetTransform("request items");

      m_oldNameText = GetComponent<Text>("old name");
      m_oldTypeText = GetComponent<Text>("old type");
      m_oldIconImg = GetComponent<Image>("old icon");
      m_oldPhaseText = GetComponent<Text>("old phase");
      m_oldEnergyText = GetComponent<Text>("old energy");
      m_oldDescTextAutoScroll = new YTextAutoScroll(GetGameObject("old desc")); 
      
      m_newNameText = GetComponent<Text>("new name");
      m_newTypeText = GetComponent<Text>("new type");
      m_newIconImg = GetComponent<Image>("new icon");
      m_newPhaseText = GetComponent<Text>("new phase");
      m_newEnergyText = GetComponent<Text>("new energy");
      m_newDescTextAutoScroll = new YTextAutoScroll(GetGameObject("new desc"));
      
      _AddListener();
    }

    public override void Open(Action callback = null, params object[] args) {
      m_card = args[0] as CardData;
      m_cardTemplate = ResourceManager.Load<CardTemplate>(m_card.id);
      
      _RefreshDetail();
      
      base.Open(callback, args);
    }

    private void _RefreshDetail() {
      if (isMaxCardPhase) {
        m_updateBtn.gameObject.SetActiveEx(false);
        for (int i = 0; i < m_requestItems.childCount; i++) {
          var tempGo = m_requestItems.GetChild(i).gameObject;
          tempGo.SetActiveEx(false);
        }
        UIManager.CloseOne(name);
        return;
      }

      m_updateBtn.gameObject.SetActiveEx(true);
      for (int i = 0; i < m_requestItems.childCount; i++) {
        var tempGo = m_requestItems.GetChild(i).gameObject;
        if (i < cardPhase.updateRequestItems.Length) {
          ItemArgs itemArgs = cardPhase.updateRequestItems[i];
          tempGo.SetActiveEx(true);
          tempGo.GetComponentInChildren<Image>().sprite = ResourceManager.Load<Sprite>
            (ResourceManager.Load<ItemTemplate>(itemArgs.id).iconFileName);
          tempGo.GetComponentInChildren<Text>().text = itemArgs.count.ToString();
        } else {
          tempGo.SetActiveEx(false);
        }
      }

      m_oldNameText.text = cardPhase.name;
      m_oldPhaseText.text = $"{m_card.phase + 1}";
      m_oldTypeText.text = m_cardTemplate.cardType.GetDescription();
      if (cardPhase.energyConsumption >= 0) {
        m_oldEnergyText.text = cardPhase.energyConsumption.ToString();
      } else {
        m_oldEnergyText.text = "X";
      }
      m_oldIconImg.sprite = ResourceManager.Load<Sprite>(cardPhase.iconFileName);
      m_oldDescTextAutoScroll.SetTextContent(StringUtil.GetActualDescription(cardPhase.description, cardPhase.buffTemplates));

      m_newNameText.text = nextCardPhase.name;
      m_newPhaseText.text = $"{m_card.phase + 2}";
      m_newTypeText.text = m_cardTemplate.cardType.GetDescription();
      if (nextCardPhase.energyConsumption >= 0) {
        m_newEnergyText.text = nextCardPhase.energyConsumption.ToString();
      } else {
        m_newEnergyText.text = "X";
      }
      m_newIconImg.sprite = ResourceManager.Load<Sprite>(nextCardPhase.iconFileName);
      m_newDescTextAutoScroll.SetTextContent(StringUtil.GetActualDescription(nextCardPhase.description, nextCardPhase.buffTemplates));
    }

    private void _AddListener() {
      GetComponent<Button>("close").onClick.AddListener(() => UIManager.CloseOne(name));
      m_updateBtn.onClick.AddListener(
        () => {
          if (!isMaxCardPhase) {
            foreach (var item in cardPhase.updateRequestItems) {
              if (DataManager.GetItemCount(item.id) < item.count) {
                MessageManager.AddMessage(new Message(Message.Style.FADE, "材料不足！", false));
                return;
              }
            }
            foreach (var item in cardPhase.updateRequestItems) {
              DataManager.ConsumeItem(item.id, item.count);
            }
            DataManager.UpdateCard(m_card);
            _RefreshDetail();
          }
        });
    }
  }
}