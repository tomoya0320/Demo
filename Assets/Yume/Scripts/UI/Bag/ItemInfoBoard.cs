using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using Yume.DB;

namespace Yume.UI {
  public class ItemInfoBoard : MonoBehaviour {
    [SerializeField]
    private Sprite _itemSelectedImg;
    [SerializeField]
    private Sprite _itemUnselectedImg;
    [SerializeField]
    private Text _nameNum;    
    [SerializeField]
    private Text _description; 
    [SerializeField]
    private Image _iconImg;
    private ItemView selectedItem;

    public async void UpdateInfoBoard() {
      if (selectedItem && selectedItem.item != null) {
        var info = DBManager.GetItemInfo(selectedItem.item.id);
        _nameNum.text = info.name + "*" + selectedItem.item.num;
        _description.text = info.description;
        _iconImg.sprite = await Addressables.LoadAssetAsync<Sprite>(info.iconFileName).Task;
        gameObject.SetActive(true);
      } else {
        gameObject.SetActive(false);
      }
    }

    public void SetSelecteItem(ItemView itemShow) {
      if (selectedItem) {
        selectedItem.GetComponent<Image>().sprite = _itemUnselectedImg;
      }
      itemShow.GetComponent<Image>().sprite = _itemSelectedImg;
      selectedItem = itemShow;
      UpdateInfoBoard();
    }
  }
}