using Yume.DB;

namespace Yume.RedPointSystem {
  public class MainItemRedPointTree : RedPointTree {
    public const string MAIN_ITEM = "main.item";

    public override RedPointTree OnInit() {
      redPointTreeType = RedPointTreeType.MAIN_ITEM;

      redPointDict.Add(MAIN_ITEM, new RedPointNode(MAIN_ITEM, null));
      foreach (var item in DBManager.instance.userItems) {
        if (!item.isViewed) {
          InitLeafNode(item.id, MAIN_ITEM);
        }
      }

      return this;
    }
  }
}