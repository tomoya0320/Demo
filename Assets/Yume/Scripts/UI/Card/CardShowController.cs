using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using Yume.Battle;
using Yume.DB;
using Yume.String;

namespace Yume.UI {
  public class CardShowController : MonoBehaviour {
    [SerializeField]
    private RectTransform _cardFolder;
    [SerializeField]
    private GridLayoutGroup _glg;

    private async void Start() {
      List<CardDB> cards = DBManager.userCards;
      int maxWidthNum = (int)((_cardFolder.rect.width + _glg.spacing.x) / (_glg.cellSize.x + _glg.spacing.x));
      float height = cards.Count / maxWidthNum * (_glg.cellSize.y + _glg.spacing.y) - _glg.spacing.y;
      if (cards.Count % maxWidthNum > 0) {
        height += _glg.cellSize.y + _glg.spacing.y;
      }
      _cardFolder.sizeDelta = new Vector2(_cardFolder.sizeDelta.x, height);
      foreach (var card in cards) {
        var cardGo = await Addressables.InstantiateAsync(StringMap.CARD_HANDBOOK, _cardFolder).Task;
        cardGo.GetComponent<Card>().OnInit(card.phase, DBManager.GetCardInfo(card.id));
      }
    }
  }
}