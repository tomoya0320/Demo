using UnityEngine;
using UnityEngine.UI;

namespace Yume.UI {
  public class UIBag : UIBase {
    public override void OnInit(UIData uiData, GameObject root) {
      base.OnInit(uiData, root);

      _AddListener();
    }

    private void _AddListener() {
      GetComponent<Button>("close").onClick.AddListener(() => UIManager.instance.CloseOne(name));
    }
  }
}