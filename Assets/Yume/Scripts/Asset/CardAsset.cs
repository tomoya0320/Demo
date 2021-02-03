using System;
using System.Collections.Generic;
using UnityEngine;
using Yume.Data;

namespace Yume.Asset {
  public class CardAsset : YScriptableObject {
    [SerializeField]
    private CardTemplate[] _cardTemplates;

    public override Dictionary<(string, Type), object> GetActualData() {
      Dictionary<(string, Type), object> data = new Dictionary<(string, Type), object>();
      foreach (var card in _cardTemplates) {
        data.Add((card.id, typeof(CardTemplate)), card);
      }
      return data;
    }
  }
}