using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using Yume.Broadcast;
using Yume.DB;
using Yume.String;

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
    private ItemInBag selectedItem;

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
        _nameNum.text = info.Value<string>(StringMap.NAME) + "*" + selectedItem.item.num;
        _description.text = info.Value<string>(StringMap.DESCRIPTION);
        _iconImg.sprite = await Addressables.LoadAssetAsync<Sprite>(info.Value<string>(StringMap.ICON_FILE_NAME)).Task;
        gameObject.SetActive(true);
      } else {
        gameObject.SetActive(false);
      }
    }

    public void SetSelectedProp(ItemInBag itemInBag) {
      if (selectedItem) {
        selectedItem.GetComponent<Image>().sprite = _itemUnselectedImg;
      }
      itemInBag.GetComponent<Image>().sprite = _itemSelectedImg;
      selectedItem = itemInBag;
      _UpdateInfo();
    }

#if TEST
    private void Update() {
      if (Input.GetKeyDown(KeyCode.A)) {
        UseItem(1);
      }
    }

    public void UseItem(int num) {
      if (selectedItem != null) {
        DBManager.ConsumeItems(new ItemDB(selectedItem.item.id, num));
      }
    }
#endif
  }
}