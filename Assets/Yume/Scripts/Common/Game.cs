using UnityEngine;
using Yume.AVG;
using Yume.Battle;
using Yume.DB;
using Yume.MailSystem;
using Yume.UI;

namespace Yume.Main {
  public class Game : SingletonMono<Game> {
    /// <summary>
    /// .asset文件的路径列表
    /// </summary>
    [SerializeField]
    private string[] _assetUrls;
    public string[] assetUrls => _assetUrls;

    private void Start() {
      //暂时先不需要检查资源的UI
      UIManager.instance.OpenOne("login");
    }

    private void Update() {
      UIManager.instance.UpdateCache();

      if (Input.GetKeyDown(KeyCode.Escape)) {
        UIManager.instance.Back();
      }

#if TEST
      if (Input.GetKeyDown(KeyCode.W)) {
        DBManager.instance.AddItems(new ItemDB("item_2_gold", 1000));
        DBManager.instance.UpdateUserData();
      }

      if (Input.GetKeyDown(KeyCode.E)) {
        DBManager.instance.ConsumeItems(true, new ItemDB("item_2_gold", 10000));
        DBManager.instance.UpdateUserData();
      }

      if (Input.GetKeyDown(KeyCode.R)) {
        DBManager.instance.AddExp(300);
        DBManager.instance.UpdateUserData();
      }

      if (Input.GetKeyDown(KeyCode.Y)) {
        DBManager.instance.AddMails(new MailDB("Test Title", "Test Title", "Welcome to FangYuanQiWu!", "admin", new ItemDB("item_1_diamond", 1000)));
        DBManager.instance.AddMails(new MailDB("Test Title2", "Test Title2", "You are alone", "admin", new ItemDB("item_1_diamond", 2000), new ItemDB("item_2_gold", 30000)));
        DBManager.instance.AddMails(new MailDB("Test Title3", "Test Title3", "I love {0}", "wzb", new ItemDB("item_1_diamond", 4000), new ItemDB("item_2_gold", 50000), new ItemDB("item_5_key", 10)));
        DBManager.instance.UpdateUserData();
      }

      if (Input.GetKeyDown(KeyCode.G)) {
        Instantiate(Resources.Load<GameObject>("AVG/Prefab/avg"), UIManager.instance.topCanvas);
        AVGController.instance.StartAVG("test1");
      }
#endif
    }
  }
}