using Yume.Battle;
using Yume.Broadcast;
using Yume.DB;

namespace Yume.UI {
  public class ItemTermGroup : UIGridPage<ItemDB, ItemTerm> {
    [UnityEngine.SerializeField]
    private ItemInfoBoard _itemInfoBoard;

    protected override void Start() {
      base.Start();
      dataList = DBManager.instance.userItems;
      ReCalMaxPageAndRefresh();
      EventCenter.AddListener(EventType.UPDATE_ITEM_INFO, ReCalMaxPageAndRefresh);
    }

    protected override void ReCalMaxPageAndRefresh() {
      base.ReCalMaxPageAndRefresh();
      _itemInfoBoard.UpdateInfoBoard();
    }

    protected override void OnTurnPageBack() {
      base.OnTurnPageBack();
      _itemInfoBoard.UpdateInfoBoard();
    }

    protected override void OnTurnPageForward() {
      base.OnTurnPageForward();
      _itemInfoBoard.UpdateInfoBoard();
    }

    private void OnDestroy() {
      EventCenter.RemoveListener(EventType.UPDATE_ITEM_INFO, ReCalMaxPageAndRefresh);
    }
  }
}