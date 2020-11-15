using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using Yume.Battle;
using Yume.Broadcast;
using Yume.String;
using Yume.DB;

namespace Yume.UI.Bag {
  public class ItemShow : MonoBehaviour {
    [SerializeField]
    private Text _num;
    [SerializeField]
    private Image _icon;
    public ItemDB item { get; private set; }

    private void Start() {
      EventCenter.AddListener(Broadcast.EventType.UPDATE_ITEM_INFO, _UpdateInfo);
      _UpdateInfo();
    }

    private void OnDestroy() {
      EventCenter.RemoveListener(Broadcast.EventType.UPDATE_ITEM_INFO, _UpdateInfo);
    }

    private async void _UpdateInfo() {
      item = DBManager.GetItemDB(transform.GetSiblingIndex());

      if (item != null) {
        _num.transform.parent.gameObject.SetActive(true);
        _num.text = StringUtil.IntegerToString(item.num);

        _icon.gameObject.SetActive(true);
        _icon.sprite = await Addressables.LoadAssetAsync<Sprite>
        (DBManager.GetItemInfo(item.id).iconFileName).Task;
      } else {
        _icon.gameObject.SetActive(false);
        _icon.sprite = null;
        _num.transform.parent.gameObject.SetActive(false);
        _num.text = StringMap.EMPTY;
      }
    }
  }
}