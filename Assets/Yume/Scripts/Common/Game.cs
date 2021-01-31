using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Yume.RedPointSystem;
using Yume.Resource;
using Yume.UI;
using Yume.Data;
using System;
using Yume.Audio;

namespace Yume.Main {
  public class Game : SingletonMono<Game> {
    /// <summary>
    /// UIRoot的对象引用
    /// </summary>
    [SerializeField]
    private ObjectContainer _uiRootObject;
    public ObjectContainer uiRootObject => _uiRootObject;
    [SerializeField]
    private Button _goToLoginBtn;
    [SerializeField]
    private Image _initProcessImg;
    [SerializeField]
    private GameObject _initPanel;
    [SerializeField]
    private Transform _audioRoot;
    public Transform audioRoot => _audioRoot;
    public float initProcess {
      set {
        _initProcessImg.fillAmount = value;
        _goToLoginBtn.gameObject.SetActiveEx(value == 1.0f);
        _initProcessImg.gameObject.SetActiveEx(value < 1.0f);
      }
    }

    protected override void OnInit() {
      _goToLoginBtn.onClick.AddListener(() => {
        UIManager.OpenOne("globalBlackFade", () => {
          Destroy(_initPanel);
          UIManager.OpenOne("login");
        });
      });
      _initProcessImg.fillAmount = 0;

      StartCoroutine(_InitAll());
    }

    private IEnumerator _InitAll() {
      // first
      yield return ResourceManager.Init();

      UIManager.Init();
      AudioManager.Init();
      RedPointManager.Init();

      // final
      initProcess = 1.0f;
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
        DataManager.AddExp(300);
      }

      if (GUILayout.Button("增加邮件", GUILayout.Width(300), GUILayout.Height(100))) {
        DataManager.AddMail(new MailData("Test Title", "Welcome to FangYuanQiWu!", "admin", new ItemData("item_1_diamond", 1000)));
        DataManager.AddMail(new MailData("Test Title2", "You are alone", "admin", new ItemData("item_1_diamond", 2000), new ItemData("item_2_gold", 30000)));
        DataManager.AddMail(new MailData("Test Title3", "I love {0}", "wzb", new ItemData("item_1_diamond", 4000), new ItemData("item_2_gold", 50000), new ItemData("item_5_key", 10)));
      }

      if (GUILayout.Button("进入测试avg", GUILayout.Width(300), GUILayout.Height(100))) {
        UIManager.OpenOne("globalBlackFade", () => {
          UIManager.OpenOne("avg", null, "avg_test1", new Action(() => Debug.Log("test1结束")));
        });
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
    }
#endif
  }
}