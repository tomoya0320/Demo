using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Yume.RedPointSystem;
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
      // 实例化初始化UI
      m_initPanel = Instantiate(Resources.Load<GameObject>("Prefab/init"), GameObject.Find("NormalCanvas").transform);
      ObjectContainer objectContainer = m_initPanel.GetComponent<ObjectContainer>();
      m_goToLoginBtn = objectContainer.GetComponent<Button>("goToLoginBtn");
      m_initProcessImg = objectContainer.GetComponent<Image>("initProcessImg");

      // 设置屏幕休眠模式
      Screen.sleepTimeout = SleepTimeout.NeverSleep;

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
      m_initProcessImg.fillAmount = 0;

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
      RedPointManager.Init();

      // Final
      initProcess = 1.0f;

      #endregion
    }

    private void Update() {
      UIManager.OnUpdate();

      if (Input.GetKeyDown(KeyCode.Escape)) {
        UIManager.Back();
      }
    }

    private void LateUpdate() {
      // 同步数据用
      DataManager.OnLateUpdate();
    }
    
#if TEST
    private bool m_isShowCheat = true;

    private void OnGUI() {
      if (!DataManager.isLogged) {
        return;
      }

      if (GUILayout.Button("作弊指令", GUILayout.Width(300), GUILayout.Height(100))) {
        m_isShowCheat = !m_isShowCheat;
      }

      if (!m_isShowCheat) {
        return;
      }

      if (GUILayout.Button("增加金币", GUILayout.Width(300), GUILayout.Height(100))) {
        DataManager.AddItem(new ItemData("item_2_gold", 1000));
      }

      if (GUILayout.Button("消耗金币", GUILayout.Width(300), GUILayout.Height(100))) {
        DataManager.ConsumeItem(new ItemData("item_2_gold", 10000));
      }

      if (GUILayout.Button("增加经验", GUILayout.Width(300), GUILayout.Height(100))) {
        DataManager.AddItem(new ItemData(StringMap.EXP_ID, 300));
      }

      if (GUILayout.Button("增加邮件", GUILayout.Width(300), GUILayout.Height(100))) {
        DataManager.AddMail(new MailData("Test Title", "Welcome to FangYuanQiWu!", "admin", new ItemData("item_1_diamond", 1000)));
        DataManager.AddMail(new MailData("Test Title2", "You are alone", "admin", new ItemData("item_1_diamond", 2000), new ItemData("item_2_gold", 30000)));
        DataManager.AddMail(new MailData("Test Title3", "I love {0}", "wzb", new ItemData("item_1_diamond", 4000), new ItemData("item_2_gold", 50000), new ItemData("item_5_key", 10)));
      }

      if (GUILayout.Button("增加角色和卡牌", GUILayout.Width(300), GUILayout.Height(100))) {
        DataManager.AddCharacter(new UnitData("char_001_brave", 0, 0));
        DataManager.AddCharacter(new UnitData("char_002_wzb", 0, 0));

        DataManager.AddCard(new CardData("card_001_atk", 0));
        DataManager.AddCard(new CardData("card_001_atk", 0));
        DataManager.AddCard(new CardData("card_001_atk", 0));
        DataManager.AddCard(new CardData("card_001_atk", 0));
        DataManager.AddCard(new CardData("card_001_atk", 0));
        DataManager.AddCard(new CardData("card_001_atk", 1));
        DataManager.AddCard(new CardData("card_001_atk", 1));
        DataManager.AddCard(new CardData("card_001_atk", 1));
        DataManager.AddCard(new CardData("card_001_atk", 0));
        DataManager.AddCard(new CardData("card_001_atk", 1));
        DataManager.AddCard(new CardData("card_001_atk", 1));
        DataManager.AddCard(new CardData("card_001_atk", 0));
        DataManager.AddCard(new CardData("card_001_atk", 1));
        DataManager.AddCard(new CardData("card_001_atk", 1));
        DataManager.AddCard(new CardData("card_001_atk", 0));
        DataManager.AddCard(new CardData("card_001_atk", 1));
      }

      if (GUILayout.Button("通关全部关卡", GUILayout.Width(300), GUILayout.Height(100))) {
        DataManager.CompleteAllLevelsTestOnly();
      }
    }
#endif
  }
}