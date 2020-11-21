using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using Yume.Battle;
using Yume.String;
using Yume.DB;

namespace Yume.UI {
  public class ItemView : MonoBehaviour, IViewInit<ItemDB> {
    [SerializeField]
    private Text _num;
    [SerializeField]
    private Image _icon;
    public ItemDB item { get; private set; }

    public async void OnInit(ItemDB data) {
      item = data;
      if (item != null) {
        _num.transform.parent.gameObject.SetActive(true);
        _num.text = StringUtil.IntegerToString(item.num);

        _icon.gameObject.SetActive(true);
        _icon.sprite = await Addressables.LoadAssetAsync<Sprite>(
                             DBManager.GetItemInfo(item.id).iconFileName).Task;
      } else {
        _icon.gameObject.SetActive(false);
        _icon.sprite = null;
        _num.transform.parent.gameObject.SetActive(false);
        _num.text = StringMap.EMPTY;
      }
    }
  }
}