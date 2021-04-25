using System;
using System.Collections.Generic;
using UnityEngine;

namespace Yume.Asset {
  public class ItemAsset : YScriptableObject {
    [SerializeField]
    private ItemTemplate[] _itemTemplates;

    public override Dictionary<(string, Type), object> GetActualData() {
      Dictionary<(string, Type), object> data = new Dictionary<(string, Type), object>();
      foreach (var item in _itemTemplates) {
        data.Add((item.id, typeof(ItemTemplate)), item);
      }
      return data;
    }
  }
}