using UnityEngine;
using UnityEngine.UI;

namespace Yume.UI {
  [ExecuteInEditMode]
  public class YImage : Image {
    [SerializeField]
    private Text _textComponent;
    public Text textComponent => _textComponent;
    
    protected override void Awake() {
      base.Awake();
      
      _textComponent = GetComponentInChildren<Text>(true);
    }
  }
}