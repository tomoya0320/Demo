using System.Collections.Generic;
using Yume.Data;
using Yume.Resource;

namespace Yume.RedPointSystem {
  public class MainItemRedPointTree : RedPointTree {
    public const string MAIN_ITEM = "main.item";
    protected override RedPointTreeType redPointTreeType => RedPointTreeType.MAIN_ITEM;

    public override void InitData() {
      redPointDict = new Dictionary<string, RedPointNode>();
      redPointDict.Add(MAIN_ITEM, new RedPointNode(redPointTreeType, MAIN_ITEM, null));
      foreach (var item in DataManager.userItems) {
        if (!ResourceManager.Load<ItemTemplate>(item.id).hideDisplay && !item.isViewed) {
          AddLeafNode(item.id, MAIN_ITEM);
        }
      }
    }
  }
}