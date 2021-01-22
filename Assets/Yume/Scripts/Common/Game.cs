using UnityEngine;
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
    private UIObject _uiRootObject;
    public UIObject uiRootObject => _uiRootObject;

    [SerializeField]
    private YScriptableObject[] _assets;
    public YScriptableObject[] assets => _assets;

    protected override void OnInit() {
      AssetLoader.Init();

      UIManager.Init();
      DataManager.Init();
      RedPointManager.Init();

      //暂时先不需要检查资源的UI
      UIManager.OpenOne("login");
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
        DataManager.AddMail(new MailData("Test Title", "Test Title", "Welcome to FangYuanQiWu!", "admin", new ItemData("item_1_diamond", 1000)));
        DataManager.AddMail(new MailData("Test Title2", "Test Title2", "You are alone", "admin", new ItemData("item_1_diamond", 2000), new ItemData("item_2_gold", 30000)));
        DataManager.AddMail(new MailData("Test Title3", "Test Title3", "I love {0}", "wzb", new ItemData("item_1_diamond", 4000), new ItemData("item_2_gold", 50000), new ItemData("item_5_key", 10)));
      }

      if (Input.GetKeyDown(KeyCode.G)) {
        Instantiate(Resources.Load<GameObject>("AVG/Prefab/avg"), UIManager.topCanvas);
        AVGController.instance.StartAVG("test1");
      }
#endif
    }
  }
}