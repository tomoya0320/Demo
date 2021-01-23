using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Yume.AVG;
using Yume.Data;
using Yume.RedPointSystem;
using Yume.Resource;
using Yume.UI;

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
    public float initProcess {
      set {
        _initProcessImg.fillAmount = value;
        _goToLoginBtn.gameObject.SetActiveEx(value == 1.0f);
        _initProcessImg.gameObject.SetActiveEx(value < 1.0f);
      }
    }

    protected override void OnInit() {
      _goToLoginBtn.onClick.AddListener(() => {
        UIManager.OpenOne("blackMask", () => {
          Destroy(_initPanel);
          UIManager.OpenOne("login");
          UIManager.CloseOne("blackMask");
        });
      });
      _initProcessImg.fillAmount = 0;

      StartCoroutine(_InitAll());
    }

    private IEnumerator _InitAll() {
      yield return ResourceManager.Init();

      UIManager.Init();
      RedPointManager.Init();
    }

    private void Update() {
      UIManager.OnUpdate();

      if (Input.GetKeyDown(KeyCode.Escape)) {
        UIManager.Back();
      }

#if TEST
      if (Input.GetKeyDown(KeyCode.W)) {
        DataManager.AddItem(new ItemData("item_2_gold", 1000));
      }

      if (Input.GetKeyDown(KeyCode.E)) {
        DataManager.ConsumeItem(new ItemData("item_2_gold", 10000));
      }

      if (Input.GetKeyDown(KeyCode.R)) {
        DataManager.AddExp(300);
      }

      if (Input.GetKeyDown(KeyCode.Y)) {
        DataManager.AddMail(new MailData("Test Title", "Welcome to FangYuanQiWu!", "admin", new ItemData("item_1_diamond", 1000)));
        DataManager.AddMail(new MailData("Test Title2", "You are alone", "admin", new ItemData("item_1_diamond", 2000), new ItemData("item_2_gold", 30000)));
        DataManager.AddMail(new MailData("Test Title3", "I love {0}", "wzb", new ItemData("item_1_diamond", 4000), new ItemData("item_2_gold", 50000), new ItemData("item_5_key", 10)));
      }

      if (Input.GetKeyDown(KeyCode.G)) {
        Instantiate(ResourceManager.Load<GameObject>("avg"), UIManager.topCanvas);
        AVGController.instance.StartAVG("test1");
      }

      if (Input.GetKeyDown(KeyCode.C)) {
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
#endif
    }
  }
}