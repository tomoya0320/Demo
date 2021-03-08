using UnityEngine;
using UnityEngine.UI;
using Yume.Data;
using Yume.RedPointSystem;

namespace Yume.UI {
  public class UILogin : UIBase {
    public override void OnInit(UITemplate uiTemplate, GameObject gameObject) {
      base.OnInit(uiTemplate, gameObject);
      
      _AddListener();
    }

    private void _AddListener() {
      GetComponent<Button>("start game").onClick.AddListener(_GoToMain);
    }

    private void _GoToMain() {
      DataManager.LoadUserData();
      RedPointManager.InitData();
      UIManager.OpenOne("globalBlackFade", () => UIManager.CloseAllAndOpenOne("main"));
    }
  }
}