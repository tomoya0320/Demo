using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using Yume.DB;

namespace Yume.UI {
  public class ShortItemInfoShow : MonoBehaviour {
    [SerializeField]
    private Image _iconImg;
    public Image iconImg => _iconImg;
    [SerializeField]
    private Text _numText;

    public async void OnInit(ItemDB item, Color iconColor) {
      _numText.text = item.num.ToString();
      _iconImg.color = iconColor;
      _iconImg.sprite = await Addressables.LoadAssetAsync<Sprite>(DBManager.GetItemInfo(item.id).iconFileName).Task;
    }

    public async void OnInit(string itemId, int itemNum, Color iconColor) {
      _numText.text = itemNum.ToString();
      _iconImg.color = iconColor;
      _iconImg.sprite = await Addressables.LoadAssetAsync<Sprite>(DBManager.GetItemInfo(itemId).iconFileName).Task;
    }
  }
}