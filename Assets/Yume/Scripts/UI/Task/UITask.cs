using UnityEngine;
using UnityEngine.UI;

namespace Yume.UI {
  public class UITask : UIBase {
    private Button m_close;

    public override void OnInit(UIData uiData, GameObject root) {
      base.OnInit(uiData, root);
      m_close = GetComponent<Button>("close");

      _AddListener();
    }

    private void _AddListener() {
      m_close.onClick.AddListener(() => UIManager.instance.CloseOne("task"));
    }
  }
}