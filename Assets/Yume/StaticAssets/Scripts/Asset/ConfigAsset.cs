using UnityEngine;
using System.Collections.Generic;
using System;

namespace Yume.Asset {
  public class ConfigAsset : YScriptableObject {
    [SerializeField]
    private string _openingAVG;
    [SerializeField]
    private string[] _initialMails;
    [SerializeField]
    private UnitArgs[] _initialChars;
    [SerializeField]
    private CardArgs[] _initialCards;
    
    public override Dictionary<(string, Type), object> GetActualData() {
      Dictionary<(string, Type), object> data = new Dictionary<(string, Type), object>();
      data.Add(("openingAVG", typeof(string)), _openingAVG);
      data.Add(("initialMails", typeof(string[])), _initialMails);
      data.Add(("initialChars", typeof(UnitArgs[])), _initialChars);
      data.Add(("initialCards", typeof(CardArgs[])), _initialCards);
      return data;
    }
  }
}