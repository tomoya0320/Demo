using System;
using UnityEngine;
using Yume.Battle;
using Yume.DB;
using Yume.MailSystem;
using Yume.UI;

namespace Yume.Master {
  public class Game : SingletonMono<Game> {
    /// <summary>
    /// .asset文件的路径列表
    /// </summary>
    [SerializeField]
    private string[] _assetUrls;
    public string[] assetUrls => _assetUrls;

    private void Start() {
      UIManager.instance.RegisterByJson();
      //暂时先不需要检查资源的UI
      UIManager.instance.OpenOne("login");
    }

    private void Update() {
      UIManager.instance.UpdateCache();

#if TEST
      if (Input.GetKeyDown(KeyCode.W)) {
        DBManager.AddItems(new ItemDB("item_2_gold", 1000));
        DBManager.UpdateUserData();
      }

      if (Input.GetKeyDown(KeyCode.E)) {
        DBManager.ConsumeItems(true, new ItemDB("item_2_gold", 1000));
        DBManager.UpdateUserData();
      }

      if (Input.GetKeyDown(KeyCode.R)) {
        DBManager.AddItems(new ItemDB("item_1_diamond", 1));
        DBManager.UpdateUserData();
      }

      if (Input.GetKeyDown(KeyCode.T)) {
        MessageManager.instance.AddMessage(new Message(Message.Style.SLIDE, "eretet"));
      }

      if (Input.GetKeyDown(KeyCode.Y)) {
        DBManager.AddMail(new MailDB("Test Title", "Welcome to FangYuanQiWu!", "admin", new ItemDB("item_1_diamond", 1000)));
        DBManager.AddMail(new MailDB("Test Title2", "You are alone", "admin", new ItemDB("item_1_diamond", 2000), new ItemDB("item_2_gold", 30000)));
        DBManager.AddMail(new MailDB("Test Title3", "I love {0}", "wzb", new ItemDB("item_1_diamond", 4000), new ItemDB("item_2_gold", 50000), new ItemDB("item_5_key", 10)));
        DBManager.UpdateUserData();
      }
#endif
    }
  }
}