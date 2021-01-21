using UnityEngine;
using Yume.AVG;
using Yume.Data;
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

    private void Start() {
      //暂时先不需要检查资源的UI
      UIManager.instance.OpenOne("login");
    }

    private void Update() {
      UIManager.instance.OnUpdate();

      if (Input.GetKeyDown(KeyCode.Escape)) {
        UIManager.instance.Back();
      }

#if TEST
      if (Input.GetKeyDown(KeyCode.W)) {
        DataManager.instance.AddItem(new ItemData("item_2_gold", 1000));
      }

      if (Input.GetKeyDown(KeyCode.E)) {
        DataManager.instance.ConsumeItem(new ItemData("item_2_gold", 10000));
      }

      if (Input.GetKeyDown(KeyCode.R)) {
        DataManager.instance.AddExp(300);
      }

      if (Input.GetKeyDown(KeyCode.Y)) {
        DataManager.instance.AddMail(new MailData("Test Title", "Test Title", "Welcome to FangYuanQiWu!", "admin", new ItemData("item_1_diamond", 1000)));
        DataManager.instance.AddMail(new MailData("Test Title2", "Test Title2", "You are alone", "admin", new ItemData("item_1_diamond", 2000), new ItemData("item_2_gold", 30000)));
        DataManager.instance.AddMail(new MailData("Test Title3", "Test Title3", "I love {0}", "wzb", new ItemData("item_1_diamond", 4000), new ItemData("item_2_gold", 50000), new ItemData("item_5_key", 10)));
      }

      if (Input.GetKeyDown(KeyCode.G)) {
        Instantiate(Resources.Load<GameObject>("AVG/Prefab/avg"), UIManager.instance.topCanvas);
        AVGController.instance.StartAVG("test1");
      }
#endif
    }
  }
}