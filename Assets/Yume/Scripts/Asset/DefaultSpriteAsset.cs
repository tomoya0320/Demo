using UnityEngine;

namespace Yume.Data {
  public class DefaultSpriteAsset : ScriptableObject {
    [SerializeField]
    private Sprite _noCharSprite;
    [SerializeField]
    private Sprite _noCardSprite;  
    [SerializeField]
    private Sprite _noItemSprite;

    public Sprite noCharSprite => _noCharSprite;
    public Sprite noCardSprite => _noCardSprite;
    public Sprite noItemSprite => _noItemSprite;
  }
}