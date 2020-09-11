using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Yume.String;

namespace Yume.DB {
  public class DBManager {
    private static UserDB s_userDB;    
    public static UserDB userDB {
      get {
        if (s_userDB == null) {
          s_userDB = new UserDB();
        }
        return s_userDB;
      }
    }
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
      if(index < 0 || index >= userDB.itemDBs.Count) {
        return null;
      }
      return userDB.itemDBs[index];
    }
  }
}