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

    private Action m_quitGame = Application.Quit;

    private void Start() {
      UIManager.instance.RegisterByJson();
      //暂时先不需要检查资源的UI
      UIManager.instance.OpenOne("login");
    }

    private void Update() {
      UIManager.instance.UpdateCache();

      if (Input.GetKeyDown(KeyCode.Escape)) {
        if (UIManager.instance.isShowing("confirmation")) {
          UIManager.instance.CloseOne("confirmation");
        } else {
          UIManager.instance.OpenOne("confirmation", null, null, "确认退出游戏？", "确认", "取消", m_quitGame, null);
        }
      }

#if TEST
      if (Input.GetKeyDown(KeyCode.W)) {
        DBManager.instance.AddItems(new ItemDB("item_2_gold", 1000));
        DBManager.instance.UpdateUserData();
      }

      if (Input.GetKeyDown(KeyCode.E)) {
        DBManager.instance.ConsumeItems(true, new ItemDB("item_2_gold", 1000));
        DBManager.instance.UpdateUserData();
      }

      if (Input.GetKeyDown(KeyCode.R)) {
        DBManager.instance.AddExp(100);
        DBManager.instance.UpdateUserData();
      }

      if (Input.GetKeyDown(KeyCode.T)) {
        MessageManager.instance.AddMessage(new Message(Message.Style.SLIDE, "eretet"));
      }

      if (Input.GetKeyDown(KeyCode.Y)) {
        DBManager.instance.AddMail(new MailDB("Test Title", "Welcome to FangYuanQiWu!", "admin", new ItemDB("item_1_diamond", 1000)));
        DBManager.instance.AddMail(new MailDB("Test Title2", "You are alone", "admin", new ItemDB("item_1_diamond", 2000), new ItemDB("item_2_gold", 30000)));
        DBManager.instance.AddMail(new MailDB("Test Title3", "I love {0}", "wzb", new ItemDB("item_1_diamond", 4000), new ItemDB("item_2_gold", 50000), new ItemDB("item_5_key", 10)));
        DBManager.instance.UpdateUserData();
      }

      if (Input.GetKeyDown(KeyCode.U)) {
        UIManager.instance.OpenOne("main");
      }
      if (Input.GetKeyDown(KeyCode.I)) {
        UIManager.instance.CloseAllAndOpenOne("login");
      }
#endif
    }
  }
}