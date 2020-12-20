using UnityEngine;
using UnityEngine.UI;
using Yume.DB;
using Yume.Resource;

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
        var info = DBManager.instance.GetItemInfo(selectedItem.item.id);
        _nameNum.text = info.name + "*" + selectedItem.item.num;
        _description.text = info.description;
        _iconImg.sprite = await ResourceManager.LoadAssetAsync<Sprite>(info.iconFileName);
        gameObject.SetActive(true);
      } else {
        gameObject.SetActive(false);
      }
    }

    public void SetSelecteItem(ItemView itemView) {
      if (selectedItem) {
        selectedItem.bottomImg.sprite = _itemUnselectedImg;
      }
      itemView.bottomImg.sprite = _itemSelectedImg;
      selectedItem = itemView;
      UpdateInfoBoard();
    }
  }
}