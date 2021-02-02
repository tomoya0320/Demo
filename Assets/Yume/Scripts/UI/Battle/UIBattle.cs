using UnityEngine;
using UnityEngine.UI;
using Yume.Data;

namespace Yume.UI {
  public class UIBattle : UIBase {
    public override void OnInit(UITemplate uiTemplate, GameObject root) {
      base.OnInit(uiTemplate, root);

      _AddListener();
    }

    private void _AddListener() {
      GetComponent<Button>("close").onClick.AddListener(() => UIManager.CloseOne(name));
      GetComponent<Button>("main_story").onClick.AddListener(() => UIManager.OpenOne("main_story"));
      GetComponent<Button>("roguelike").onClick.AddListener(() => UIManager.OpenOne("roguelike"));
      GetComponent<Button>("pvp").onClick.AddListener(() => UIManager.OpenOne("pvp"));
    }
  }
}