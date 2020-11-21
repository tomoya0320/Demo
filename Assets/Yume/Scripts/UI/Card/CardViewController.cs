using Yume.Battle;
using Yume.Broadcast;
using Yume.DB;

namespace Yume.UI {
  public class CardViewController : UIGridPage<CardDB,CardView> {
    private void Start() {
      data = DBManager.userCards;
      ReCalMaxPageAndRefresh();
      EventCenter.AddListener(EventType.UPDATE_CARD_INFO, ReCalMaxPageAndRefresh);
    }

    private void OnDestroy() {
      EventCenter.RemoveListener(EventType.UPDATE_CARD_INFO, ReCalMaxPageAndRefresh);
    }
  }
}