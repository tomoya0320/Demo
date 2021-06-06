using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

namespace Yume {
  public class Game : SingletonMono<Game> {
    private Text m_tipText;
    private Image m_processImg;
    public event Action onUpdate;
    private Button m_goToLoginBtn;
    private GameObject m_initPanel;
    public float process {
      set => m_processImg.fillAmount = value;
    }

#if MOBILE_CONSOLE
    [SerializeField]
    private GameObject _lunarConsolePrefab;
#endif

    protected override void OnInit() {
#if MOBILE_CONSOLE
      Instantiate(_lunarConsolePrefab);
#endif
      // 设置屏幕休眠模式
      Screen.sleepTimeout = SleepTimeout.NeverSleep;
      // 实例化初始化UI
      var parent = GameObject.Find("UIRoot/NormalCanvas").transform;
      var initPanelPrefab = Resources.Load<GameObject>("init");
      m_initPanel = ObjectUtil.Instantiate(initPanelPrefab, parent);
      m_initPanel.name = initPanelPrefab.name;
      var objectContainer = m_initPanel.GetComponent<ObjectContainer>();
      m_tipText = objectContainer.GetComponent<Text>("tip");
      m_processImg = objectContainer.GetComponent<Image>("processImg");
      m_goToLoginBtn = objectContainer.GetComponent<Button>("goToLoginBtn");
      // 进入主界面的点击事件
      m_goToLoginBtn.onClick.AddListener(
        () => {
          Action callback = () => {
            Destroy(m_initPanel);
            UIManager.OpenOne("main");
          };
          if (DataManager.LoadUserData()) {
            UIManager.GlobalFade(GlobalFadeType.CIRCLE_FADE, callback);
          } else {
            Action action = () => DataManager.UpdateGuide(ConditionType.FIRST_PLAY);
            callback = action + callback;
            UIManager.GlobalFade(
              GlobalFadeType.BLACK_FADE,
              () => UIManager.OpenOne("avg", null, StringMap.AVG_OPENING, callback)
            );
          }
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
      MessageManager.Init(); // 依赖UIManager
      GuideManager.Init(); // 依赖UIManager
      EffectManager.Init();
      DataManager.Init();
      // 清理资源
      yield return Resources.UnloadUnusedAssets();
      // 完成初始化
      process = 1.0f;
      // 隐藏和显示相应的UI
      m_processImg.gameObject.SetActiveEx(false);
      m_goToLoginBtn.gameObject.SetActiveEx(true);
    }

    private void Update() {
      onUpdate?.Invoke();
    }

#if TEST
    private bool m_showCheatPanel;

    private void LateUpdate() {
      if (Input.GetKeyDown(KeyCode.F1)) {
        m_showCheatPanel = !m_showCheatPanel;
      }
    }

    private void OnGUI() {
      if (m_showCheatPanel) {
        if (GUILayout.Button("清除存档", GUILayout.Width(200), GUILayout.Height(80))) {
          PlayerPrefs.DeleteKey(DataManager.USER_DB_KEY);
        }

        if (GUILayout.Button("战斗胜利", GUILayout.Width(200), GUILayout.Height(80))) {
          Battle.BattleManager.Finish(Battle.BattleResult.WIN);
        }

        if (GUILayout.Button("战斗失败", GUILayout.Width(200), GUILayout.Height(80))) {
          Battle.BattleManager.Finish(Battle.BattleResult.LOSE);
        }
      }
    }
#endif
  }
}