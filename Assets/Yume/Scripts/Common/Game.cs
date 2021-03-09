using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Yume.RedDotSystem;
using Yume.Resource;
using Yume.UI;
using Yume.Data;
using Yume.Audio;
using Yume.Hotfix;

namespace Yume.Main {
  public class Game : SingletonMono<Game> {
    private Button m_goToLoginBtn;
    private Image m_initProcessImg;
    private GameObject m_initPanel;
    public float initProcess {
      set {
        m_initProcessImg.fillAmount = value;
        m_goToLoginBtn.gameObject.SetActiveEx(value >= 1.0f);
        m_initProcessImg.gameObject.SetActiveEx(value < 1.0f);
      }
    }

    protected override void OnInit() {
      // 设置屏幕休眠模式
      Screen.sleepTimeout = SleepTimeout.NeverSleep;

      // 实例化初始化UI
      var parent = GameObject.Find("NormalCanvas").transform;
      var initPanelPrefab = Resources.Load<GameObject>("Init");
      m_initPanel = Instantiate(initPanelPrefab, parent);
      var objectContainer = m_initPanel.GetComponent<ObjectContainer>();
      m_goToLoginBtn = objectContainer.GetComponent<Button>("goToLoginBtn");
      m_initProcessImg = objectContainer.GetComponent<Image>("initProcessImg");

      // 进入登录界面的点击事件
      m_goToLoginBtn.onClick.AddListener(
        () => {
          UIManager.OpenOne(
            "globalBlackFade", () => {
              Destroy(m_initPanel);
              UIManager.OpenOne("login");
            }
          );
        }
      );

      StartCoroutine(_InitAll());
    }

    private IEnumerator _InitAll() {
      #region 异步

      // Hotfix
      yield return HotfixManager.Init(0, 0.1f);

      // Resource
      yield return ResourceManager.Init(0.1f, 0.9f);

      #endregion

      #region 同步

      UIManager.Init();
      AudioManager.Init();
      RedDotManager.Init();

      #endregion

      // Final
      initProcess = 1.0f;
    }

    private void Update() {
      UIManager.OnUpdate();

      if (Input.GetKeyDown(KeyCode.Escape)) {
        UIManager.Back();
      }
    }

    private void LateUpdate() {
      DataManager.OnLateUpdate();
    }

#if BETA
    private void OnGUI() {
      if (GUILayout.Button("打开控制台", GUILayout.Width(200), GUILayout.Height(80))) {
        LunarConsolePlugin.LunarConsole.Show();
      }   
      
      if (GUILayout.Button("清除存档", GUILayout.Width(200), GUILayout.Height(80))) {
        PlayerPrefs.DeleteKey(DataManager.USER_DB_KEY);
      }
    }
#endif
  }
}