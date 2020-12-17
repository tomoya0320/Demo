using System;
using UnityEngine;
using Yume.Battle;
using Yume.DB;
using Yume.MailSystem;
using Yume.UI;

namespace Yume.Master {
  public class Game : SingletonMono<Game> {
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
      if (Input.GetKeyDown(KeyCode.U)) {
        UIManager.instance.CloseOne("login");
      }
      if (Input.GetKeyDown(KeyCode.I)) {
        UIManager.instance.OpenOne("login");
      }     
      
      if (Input.GetKeyDown(KeyCode.O)) {
        UIManager.instance.OpenOne("childUITest");
      }    
      
      if (Input.GetKeyDown(KeyCode.P)) {
        UIManager.instance.CloseOne("childUITest");
      }
#endif
    }
  }
}