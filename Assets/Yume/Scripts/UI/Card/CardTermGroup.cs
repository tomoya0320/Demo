using Yume.Battle;
using Yume.Broadcast;
using Yume.DB;

namespace Yume.UI {
  public class CardTermGroup : UIGridPage<CardDB,CardTerm> {
    protected override void Start() {
      base.Start();
      dataList = DBManager.instance.userCards;
      ReCalMaxPageAndRefresh();
      EventCenter.AddListener(EventType.UPDATE_CARD_INFO, ReCalMaxPageAndRefresh);
    }

    private void OnDestroy() {
      EventCenter.RemoveListener(EventType.UPDATE_CARD_INFO, ReCalMaxPageAndRefresh);
    }
  }
}