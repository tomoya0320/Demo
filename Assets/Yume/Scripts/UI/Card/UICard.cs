using UnityEngine;
using UnityEngine.UI;
using Yume.Battle;
using Yume.DB;
using Yume.Resource;
using Yume.String;

namespace Yume.UI {
  public class UICard : UIBase {
    private class UICardTerm {
      private CardDB m_card;
      private Text m_nameText;
      private Text m_typeText;
      private Image m_iconImg;
      private Text m_phaseText;
      private Text m_energyText;
      private Text m_descriptionText;

      public UICardTerm(CardDB cardDB, GameObject root) {
        var uiObject = root.GetComponent<UIObject>();
        m_nameText = uiObject.GetComponent<Text>("NameText");
        m_typeText = uiObject.GetComponent<Text>("TypeText");
        m_iconImg = uiObject.GetComponent<Image>("IconImage");
        m_phaseText = uiObject.GetComponent<Text>("PhaseText");
        m_energyText = uiObject.GetComponent<Text>("EnergyText");
        m_descriptionText = uiObject.GetComponent<Text>("DescriptionText");

        m_card = cardDB;

        _OnInitInternal();
      }

      private async void _OnInitInternal() {
        if (m_card != null) {
          CardInfo cardInfo = DBManager.instance.GetCardInfo(m_card.id);
          m_nameText.text = cardInfo.name;
          m_phaseText.text = m_card.phase.ToString();
          m_typeText.text = StringUtil.GetCardTypeName(cardInfo.type);
          m_energyText.text = cardInfo.phases[m_card.phase].energyConsumption.ToString();
          m_descriptionText.text = StringUtil.GetActualDescription(cardInfo.phases[m_card.phase].description,
                                                                  cardInfo.phases[m_card.phase].blackboard);
          m_iconImg.sprite = await ResourceManager.LoadAssetAsync<Sprite>(cardInfo.iconFileName);
        }
      }
    }
    private class UICardGroup {

    }

    public override void OnInit(UIData uiData, GameObject root) {
      base.OnInit(uiData, root);

      _AddListener();
    }

    private void _AddListener() {
      GetComponent<Button>("close").onClick.AddListener(() => UIManager.instance.CloseOne(name));
    }
  }
}