using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEditor.PackageManager;
using UnityEngine;
using Yume.Broadcast;
using Yume.Server;
using Yume.String;
using Yume.UI;

namespace Yume.DB {
  public class DBManager {
    private enum PlayMode {
      LOCAL, REMOTE
    }

    private static string localUserDBPath {
      get {
        return Path.Combine(Application.persistentDataPath, "savedata");
      }
    }

    private static UserDB s_userDB;
    public static string userAccount {
      get {
        if (s_userDB == null) {
          return StringMap.EMPTY;
        } else {
          return s_userDB.account;
        }
      }
    }
    public static List<ItemDB> userItems {
      get {
        if (s_userDB == null) {
          return null;
        } else {
          return s_userDB.items;
        }
      }
    }
    private static PlayMode s_playMode;

    private static List<JToken> s_itemJsons;
    private static List<JToken> itemJsons {
      get {
        if (s_itemJsons == null) {
          JObject json = JObject.Parse(Resources.Load<TextAsset>(StringMap.ITEMS).text);
          s_itemJsons = json[StringMap.ITEMS].Value<JArray>().ToList();
        }
        return s_itemJsons;
      }
    }

    private DBManager() { }

    public static JToken GetItemInfo(string id) {
      JToken itemInfo = itemJsons.Find(j => j[StringMap.ID].Value<string>() == id);
      return itemInfo;
    }

    public static ItemDB GetItemDB(int index) {
      if (index < 0 || index >= s_userDB.items.Count) {
        return null;
      }
      return s_userDB.items[index];
    }

    public static ItemDB GetItemDB(string id) {
      return s_userDB.items.Find(item => item.id == id);
    }

    public static void LoadUserDB(JObject json = null) {
      if (json == null) {
        s_playMode = PlayMode.LOCAL;
        FileStream fs;
        BinaryFormatter bf = new BinaryFormatter();
        if (File.Exists(localUserDBPath)) {
          fs = new FileStream(localUserDBPath, FileMode.Open);
          s_userDB = (UserDB)bf.Deserialize(fs);
        } else {
          s_userDB = new UserDB();
          fs = File.Open(localUserDBPath, FileMode.Create);
          //序列化     
          bf.Serialize(fs, s_userDB);
        }
        fs.Close();
      } else {
        s_playMode = PlayMode.REMOTE;
        s_userDB = JsonConvert.DeserializeObject<UserDB>(json.ToString());
      }
    }

    public static void ConsumeItems(params ItemDB[] items) {
      ItemDB tempItem;
      foreach (var item in items) {
        tempItem = GetItemDB(item.id);
        if (tempItem != null && tempItem.num >= item.num) {
          tempItem.num -= item.num;
        }
      }
      _Save(new { account = userAccount, items = userItems, types = new string[] { "items" } },
      () => EventCenter.Broadcast(Broadcast.EventType.UPDATE_ITEM_INFO));
    }

    private static void _Save<T>(T datas, Action onSucceed) where T : class {
      ServerConnection.instance.Save(datas,
       request => {
         string error = StringMap.EMPTY;
         if (!string.IsNullOrEmpty(request.error)) {
           error = "连接出错！" + request.error;
         } else {
           var json = JObject.Parse(request.downloadHandler.text);
           int code = json["code"].Value<int>();
           if (code == -1) {
             error = "更新用户数据失败！";
           }
         }
         if (string.IsNullOrEmpty(error)) {
           onSucceed?.Invoke();
         } else {
           UIController.instance.OpenDifference(error, StringMap.RETRY, StringMap.RETURN_TO_LOGIN,
           () => _Save(datas, onSucceed), () => { });
         }
       });
    }
  }
}