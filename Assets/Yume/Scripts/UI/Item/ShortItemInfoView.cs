using UnityEngine;
using UnityEngine.UI;
using Yume.Battle;
using Yume.DB;
using Yume.Resource;

namespace Yume.UI {
  public class ShortItemInfoView : MonoBehaviour {
    [SerializeField]
    private Image _iconImg;
    public Image iconImg => _iconImg;
    [SerializeField]
    private Text _numText;

    public async void OnInit(ItemDB item, Color iconColor) {
      _numText.text = item.num.ToString();
      _iconImg.color = iconColor;
      _iconImg.sprite = await ResourceManager.LoadAssetAsync<Sprite>(DBManager.instance.GetItemInfo(item.id).iconFileName);
    }

    public async void OnInit(string itemId, int itemNum, Color iconColor) {
      _numText.text = itemNum.ToString();
      _iconImg.color = iconColor;
      _iconImg.sprite = await ResourceManager.LoadAssetAsync<Sprite>(DBManager.instance.GetItemInfo(itemId).iconFileName);
    }
  }
}