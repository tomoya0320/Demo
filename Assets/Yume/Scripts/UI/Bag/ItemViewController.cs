using Yume.Battle;
using Yume.Broadcast;
using Yume.DB;

namespace Yume.UI {
  public class ItemViewController : UIGridPage<ItemDB, ItemView> {
    [UnityEngine.SerializeField]
    private ItemInfoBoard _itemInfoBoard;

    private void Start() {
      data = DBManager.userItems;
      ReCalMaxPageAndRefresh();
      EventCenter.AddListener(EventType.UPDATE_ITEM_INFO, ReCalMaxPageAndRefresh);
    }

    protected override void ReCalMaxPageAndRefresh() {
      base.ReCalMaxPageAndRefresh();
      _itemInfoBoard.UpdateInfoBoard();
    }

    public override void TurnPageBack() {
      base.TurnPageBack();
      _itemInfoBoard.UpdateInfoBoard();
    }

    public override void TurnPageForward() {
      base.TurnPageForward();
      _itemInfoBoard.UpdateInfoBoard();
    }

    private void OnDestroy() {
      EventCenter.RemoveListener(EventType.UPDATE_ITEM_INFO, ReCalMaxPageAndRefresh);
    }
  }
}