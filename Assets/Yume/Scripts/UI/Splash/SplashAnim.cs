using UnityEngine;
using UnityEngine.UI;

namespace Yume.UI {
  public class SplashAnim : UIBase {
    private Button m_startBtn;
    private Animation m_animation;

    public override void OnInit(UIData uiData, GameObject root) {
      base.OnInit(uiData, root);
      m_startBtn = uiObject.GetComponent<Button>("startBtn");
      m_animation = uiObject.GetComponent<Animation>("animation");

      m_startBtn.onClick.AddListener(() => UIManager.instance.OpenUI("login"));
    }

    public override void Close(params object[] paramArray) {
      m_animation.Stop();
    }

    public override void Open(params object[] paramArray) {
      m_animation.Play();
    }
  }
}