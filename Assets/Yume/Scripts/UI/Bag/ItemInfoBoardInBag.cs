using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using Yume.Broadcast;
using Yume.DB;

namespace Yume.UI.Bag {
  public class ItemInfoBoardInBag : MonoBehaviour {
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
    private ItemShow selectedItem;

    private void Start() {
      EventCenter.AddListener(Broadcast.EventType.UPDATE_ITEM_INFO, _UpdateInfo);
      _UpdateInfo();
    }

    private void OnDestroy() {
      EventCenter.RemoveListener(Broadcast.EventType.UPDATE_ITEM_INFO, _UpdateInfo);
    }

    private async void _UpdateInfo() {
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

    public void SetSelecteItem(ItemShow itemShow) {
      if (selectedItem) {
        selectedItem.GetComponent<Image>().sprite = _itemUnselectedImg;
      }
      itemShow.GetComponent<Image>().sprite = _itemSelectedImg;
      selectedItem = itemShow;
      _UpdateInfo();
    }

#if TEST
    private void Update() {
      if (Input.GetKeyDown(KeyCode.A)) {
        _UseItem(1);
      }      
      
      if (Input.GetKeyDown(KeyCode.S)) {
        _AddItem(1);
      }

      if (Input.GetKeyDown(KeyCode.G)) {
        DBManager.AddItems(new ItemDB("item_2_gold", 1000));
      }      
      
      if (Input.GetKeyDown(KeyCode.C)) {
        DBManager.AddItems(new ItemDB("item_1_diamond", 1));
      }
    }

    private void _UseItem(int num) {
      if (selectedItem != null) {
        DBManager.ConsumeItems(true, new ItemDB(selectedItem.item.id, num));
      }
    }    
    
    private void _AddItem(int num) {
      if (selectedItem != null) {
        DBManager.AddItems(new ItemDB(selectedItem.item.id, num));
      }
    }
#endif
  }
}