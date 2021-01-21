using Yume.Data;

namespace Yume.RedPointSystem {
  public class MainItemRedPointTree : RedPointTree {
    public const string MAIN_ITEM = "main.item";

    public override RedPointTree OnInit() {
      redPointTreeType = RedPointTreeType.MAIN_ITEM;

      redPointDict.Add(MAIN_ITEM, new RedPointNode(redPointTreeType, MAIN_ITEM, null));
      foreach (var item in DataManager.instance.userItems) {
        if (!item.isViewed) {
          AddLeafNode(item.id, MAIN_ITEM);
        }
      }

      return this;
    }
  }
}