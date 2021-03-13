using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;
using Yume.RedDotSystem;
using Yume.Resource;
using Yume.UI;
using Yume.Data;
using Yume.Audio;
using Yume.Hotfix;

namespace Yume {
  public class Game : SingletonMono<Game> {
    private Text m_tipText;
    private Image m_processImg;
    private Button m_goToLoginBtn;
    private GameObject m_initPanel;
    public float process {
      set => m_processImg.fillAmount = value;
    }
#if BETA
    public GameObject lunarConsolePrefab;
#endif

    protected override void OnInit() {
#if BETA
      Instantiate(lunarConsolePrefab);
#endif
      // 设置屏幕休眠模式
      Screen.sleepTimeout = SleepTimeout.NeverSleep;
      // 实例化初始化UI
      var parent = GameObject.Find("NormalCanvas").transform;
      var initPanelPrefab = Resources.Load<GameObject>("Init");
      m_initPanel = Instantiate(initPanelPrefab, parent);
      var objectContainer = m_initPanel.GetComponent<ObjectContainer>();
      m_tipText = objectContainer.GetComponent<Text>("tip");
      m_processImg = objectContainer.GetComponent<Image>("processImg");
      m_goToLoginBtn = objectContainer.GetComponent<Button>("goToLoginBtn");
      // 进入主界面的点击事件
      m_goToLoginBtn.onClick.AddListener(
        () => {
          DataManager.LoadUserData();
          RedDotManager.InitData();
          UIManager.OpenOne(
            "globalCircleBlackFade",
            () => {
              Destroy(m_initPanel);
              UIManager.DestroyAllAndOpenOne("main");
            },
            ScreenUtil.mousePosNormalized
          );
        }
      );
      // 初始化
      StartCoroutine(_Init());
    }

    private IEnumerator _Init() {
      process = 0;
      m_tipText.text = "检查游戏资源中";
      m_processImg.gameObject.SetActiveEx(true);
      
      yield return Addressables.InitializeAsync();

      var sizehandle = Addressables.GetDownloadSizeAsync("Download");
      yield return sizehandle;

      if (sizehandle.Result > 0) {
        m_tipText.text = $"正在下载游戏资源({(sizehandle.Result / 1024.0f / 1024.0f):0.00}MB)";
        var downloadHandle = Addressables.DownloadDependenciesAsync("Download");
        while (!downloadHandle.IsDone) {
          process = downloadHandle.PercentComplete;
          yield return null;
        }
        if (downloadHandle.Status != AsyncOperationStatus.Succeeded) {
          m_tipText.text = "下载出错请重启游戏";
          yield break;
        }
      }
      
      process = 0;
      m_tipText.text = "游戏初始化中";
      
      // Hotfix
      yield return HotfixManager.Init(0, 0.1f);
      // Resource
      yield return ResourceManager.Init(0.1f, 0.9f);
      
      UIManager.Init();
      AudioManager.Init();
      RedDotManager.Init();
      
      process = 1.0f;
      
      yield return Resources.UnloadUnusedAssets();

      m_processImg.gameObject.SetActiveEx(false);
      m_goToLoginBtn.gameObject.SetActiveEx(true);
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