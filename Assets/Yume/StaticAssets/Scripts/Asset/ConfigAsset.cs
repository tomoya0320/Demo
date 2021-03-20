using UnityEngine;
using System.Collections.Generic;
using System;

namespace Yume.Asset {
  public class ConfigAsset : YScriptableObject {
    [SerializeField]
    private Sprite _noSprite;   
    [SerializeField]
    private Sprite _noCardSprite;    
    [SerializeField]
    private Sprite _noCharSprite;
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
      data.Add(("noSprite", typeof(Sprite)), _noSprite);
      data.Add(("noCardSprite", typeof(Sprite)), _noCardSprite);
      data.Add(("noCharSprite", typeof(Sprite)), _noCharSprite);
      data.Add(("openingAVG", typeof(string)), _openingAVG);
      data.Add(("initialMails", typeof(string[])), _initialMails);
      data.Add(("initialChars", typeof(UnitArgs[])), _initialChars);
      data.Add(("initialCards", typeof(CardArgs[])), _initialCards);
      return data;
    }
  }
}