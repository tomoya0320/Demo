using System.Collections.Generic;
using UnityEngine;
using Yume.Resource;

namespace Yume.DB {
  public class DefaultSpriteAsset : YScriptableObject {
    [SerializeField]
    private Sprite _noCharSprite;
    [SerializeField]
    private Sprite _noCardSprite;  
    [SerializeField]
    private Sprite _noItemSprite;

    public Sprite noCharSprite => _noCharSprite;
    public Sprite noCardSprite => _noCardSprite;
    public Sprite noItemSprite => _noItemSprite;

    public override Dictionary<string, object> GetAssets() {
      Dictionary<string, object> assets = new Dictionary<string, object>();
      assets.Add(nameof(noCharSprite), noCharSprite);
      assets.Add(nameof(noCardSprite), noCardSprite);
      assets.Add(nameof(noItemSprite), noItemSprite);

      return assets;
    }
  }
}