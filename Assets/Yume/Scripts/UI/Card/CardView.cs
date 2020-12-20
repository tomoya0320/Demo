using UnityEngine;
using UnityEngine.UI;
using Yume.Battle;
using Yume.DB;
using Yume.String;
using Yume.Resource;

namespace Yume.UI {
  public class CardView : MonoBehaviour, IViewInit<CardDB> {
    [SerializeField]
    private Image _icon;
    [SerializeField]
    private Text _energyText;
    [SerializeField]
    private Text _phaseText;
    [SerializeField]
    private Text _descriptionText;
    [SerializeField]
    private Text _nameText;
    [SerializeField]
    private Text _typeText;
    private Sprite m_originalSprite;

    private void Awake() {
      m_originalSprite = _icon.sprite;
    }

    public async void OnInit(CardDB cardDB) {
      if (cardDB == null) {
        _Reset();
      } else {
        CardInfo cardInfo = DBManager.instance.GetCardInfo(cardDB.id);
        _nameText.text = cardInfo.name;
        _phaseText.text = cardDB.phase.ToString();
        _typeText.text = StringUtil.GetCardTypeName(cardInfo.type);
        _energyText.text = cardInfo.phases[cardDB.phase].energyConsumption.ToString();
        _descriptionText.text = StringUtil.GetActualDescription(cardInfo.phases[cardDB.phase].description,
                                                                cardInfo.phases[cardDB.phase].blackboard);
        _icon.sprite = await ResourceManager.LoadAssetAsync<Sprite>(cardInfo.iconFileName);
      }
    }

    private void _Reset() {
      _nameText.text = StringMap.EMPTY;
      _phaseText.text = StringMap.EMPTY;
      _typeText.text = StringMap.EMPTY;
      _energyText.text = StringMap.EMPTY;
      _descriptionText.text = StringMap.EMPTY;
      _icon.sprite = m_originalSprite;
    }
  }
}