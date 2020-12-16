using UnityEngine;
using Yume.Battle;
using Yume.DB;
using Yume.MailSystem;
using Yume.Singleton;
using Yume.UI;

namespace Yume.Master {
  public class Game : SingletonMono<Game> {
    private void Start() {
      UIManager.instance.RegisterUIByJson();
      UIManager.instance.OpenUI("splash");
    }

    private void Update() {
#if TEST
      if (Input.GetKeyDown(KeyCode.A)) {
        DBManager.AddItems(new ItemDB("item_2_gold", 1000));
        DBManager.UpdateUserData();
      }

      if (Input.GetKeyDown(KeyCode.S)) {
        DBManager.ConsumeItems(true, new ItemDB("item_2_gold", 1000));
        DBManager.UpdateUserData();
      }

      if (Input.GetKeyDown(KeyCode.D)) {
        DBManager.AddItems(new ItemDB("item_1_diamond", 1));
        DBManager.UpdateUserData();
      }

      if (Input.GetKeyDown(KeyCode.C)) {
        DBManager.setUserToken = "1d";
      }

      if (Input.GetKeyDown(KeyCode.M)) {
        DBManager.AddMail(new MailDB("Test Title", "Welcome to FangYuanQiWu!", "admin", new ItemDB("item_1_diamond", 1000)));
        DBManager.AddMail(new MailDB("Test Title2", "You are alone", "admin", new ItemDB("item_1_diamond", 2000), new ItemDB("item_2_gold", 30000)));
        DBManager.AddMail(new MailDB("Test Title3", "I love {0}", "wzb", new ItemDB("item_1_diamond", 4000), new ItemDB("item_2_gold", 50000), new ItemDB("item_5_key", 10)));
        DBManager.UpdateUserData();
      }

      if (Input.GetKeyDown(KeyCode.E)) {
        DBManager.AddExp(300);
        DBManager.UpdateUserData();
      }

      if (Input.GetKeyDown(KeyCode.L)) {
        MessageManager.instance.AddMessage(new Message(Message.Style.SLIDE, "testtesttesttesttesttesttesttesttesttesttesttest", null));
      }

      if (Input.GetKeyDown(KeyCode.G)) {
        UIManager.instance.OpenUI("avgTest");
      }

      if (Input.GetKeyDown(KeyCode.O)) {
        DBManager.userCards.Add(new CardDB("card_001_atk", 0));
        Broadcast.EventCenter.Broadcast(Broadcast.EventType.UPDATE_CARD_INFO);
      }

      if (Input.GetKeyDown(KeyCode.P)) {
        if (DBManager.userCards.Count > 0) {
          DBManager.userCards.RemoveAt(0);
          Broadcast.EventCenter.Broadcast(Broadcast.EventType.UPDATE_CARD_INFO);
        }
      }

      if (Input.GetKeyDown(KeyCode.J)) {
        Debug.Log(DBManager.GetCharJson("char_001_brave"));
        Debug.Log(DBManager.GetEnemyJson("enemy_001_slime"));
      }
#endif
    }
  }
}