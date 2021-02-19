using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Yume.Data;
using Yume.Resource;
using Yume.UI;

namespace Yume.Server {
  public static class ServerManager {
    private enum Methods {
      LOGIN, REGISTER, ALTERPSD, UPDATE
    }

    private static readonly string userServerUrl = ResourceManager.Load<string>("userServerUrl");

    public static void Login(string account, string password, Action<UnityWebRequest> onCompleted) {
      JObject json = JObject.FromObject(new { account, password });
      json.Add("method", Methods.LOGIN.ToString());
      //添加token以验证同步
      json.Add("token", DateTime.Now.Ticks + SystemInfo.deviceUniqueIdentifier);
      CoroutineHelper.instance.Run(_SendWebRequest(userServerUrl, json, onCompleted));
    }

    public static void Register(string name, string account, string password, Action<UnityWebRequest> onCompleted) {
      //Java那边的Json有个bug，转数组时输入的Json字符串不能是空（""）
      //否则会出现类似"[null]"这样的结果，导致反序列化得到的List会变成Count为1且第一个元素为null
      //update：换了一种方式初始化(直接new UserDB())
#if BETA
      UserData user = new UserData(name/*暂时测试用*/, account, password);
      user.chars.Add(new UnitData("char_001", 20, 0));
      user.cards.Add(new CardData("card_001", 0));
      user.cards.Add(new CardData("card_001", 0));
      user.cards.Add(new CardData("card_001", 1));
      user.cards.Add(new CardData("card_001", 1));
      user.cards.Add(new CardData("card_002", 0));
      user.cards.Add(new CardData("card_002", 0));
      user.cards.Add(new CardData("card_002", 1));
      user.cards.Add(new CardData("card_002", 1));
      user.cards.Add(new CardData("card_003", 0));
      user.cards.Add(new CardData("card_003", 0));
      user.cards.Add(new CardData("card_003", 1));
      user.cards.Add(new CardData("card_003", 1));
      user.cards.Add(new CardData("card_004", 0));
      user.cards.Add(new CardData("card_004", 0));
      user.cards.Add(new CardData("card_004", 1));
      user.cards.Add(new CardData("card_004", 1));
      foreach (var card in user.cards) {
        card.guid = user.validGuid++;
      }
      user.mails.Add(new MailData("内测邮件", "亲爱的{0}，欢迎参加本次内测", "管理员", new ItemData("item_1_diamond", 4000), new ItemData("item_2_gold", 50000), new ItemData("item_5_key", 10)));
      JObject json = JObject.FromObject(new { user });
#else
      JObject json = JObject.FromObject(new { user = new UserData(name/*暂时测试用*/, account, password) });
#endif
      json.Add("method", Methods.REGISTER.ToString());
      CoroutineHelper.instance.Run(_SendWebRequest(userServerUrl, json, onCompleted));
    }

    public static void UpdateUserData(string data, Action<UnityWebRequest> onCompleted) {
      JObject json = JObject.Parse(data);
      json.Add("token", DataManager.userToken);
      json.Add("method", Methods.UPDATE.ToString());
      CoroutineHelper.instance.Run(_SendWebRequest(userServerUrl, json, onCompleted));
    }

    public static void AlterPassword(string account, string oldPassword, string newPassword, Action<UnityWebRequest> onCompleted) {
      JObject json = JObject.FromObject(new { account, oldPassword, newPassword });
      json.Add("method", Methods.ALTERPSD.ToString());
      CoroutineHelper.instance.Run(_SendWebRequest(userServerUrl, json, onCompleted));
    }

    private static IEnumerator _SendWebRequest(string url, JObject data, Action<UnityWebRequest> onCompleted) {
      UnityWebRequest request = UnityWebRequest.Get(url);
      byte[] bytes = Encoding.UTF8.GetBytes(StringUtil.StringToBase64(data.ToString()));
      request.uploadHandler = new UploadHandlerRaw(bytes);
      request.downloadHandler = new DownloadHandlerBuffer();

      UIManager.SetGlobalInteractable(false);
      yield return request.SendWebRequest();
      UIManager.SetGlobalInteractable(true);

      onCompleted?.Invoke(request);
    }
  }
}