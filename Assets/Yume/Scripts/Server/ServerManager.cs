using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Yume.DB;
using Yume.Resource;
using Yume.String;
using Yume.Tool;
using Yume.UI;

namespace Yume.Server {
  public class ServerManager : Singleton<ServerManager> {
    private enum Methods {
      LOGIN, REGISTER, ALTERPSD, UPDATE
    }

    private string userServerUrl {
      get {
        return AssetLoader.instance.GetValue<string>("userServerUrl");
      }
    }
  
    public void Login(string account, string password, Action<UnityWebRequest> onCompleted) {
      JObject json = JObject.FromObject(new { account, password });
      json.Add("method", Methods.LOGIN.ToString());
      //添加token以验证同步
      json.Add("token", DateTime.Now.ToString() + SystemInfo.deviceUniqueIdentifier);
      CoroutineHelper.instance.RunCoroutine(_SendWebRequest(userServerUrl, json, onCompleted));
    }

    public void Register(string name, string account, string password, Action<UnityWebRequest> onCompleted) {
      //Java那边的Json有个bug，转数组时输入的Json字符串不能是空（""）
      //否则会出现类似"[null]"这样的结果，导致反序列化得到的List会变成Count为1且第一个元素为null
      //update：换了一种方式初始化(直接new UserDB())
      JObject json = JObject.FromObject(new { userDB = new UserDB(name/*暂时测试用*/, account, password) });
      json.Add("method", Methods.REGISTER.ToString());
      CoroutineHelper.instance.RunCoroutine(_SendWebRequest(userServerUrl, json, onCompleted));
    }

    public void UpdateUserData(JObject data, Action<UnityWebRequest> onCompleted) {
      data.Add("method", Methods.UPDATE.ToString());
      //添加token以验证同步
      data.Add("token", DBManager.instance.userToken);
      CoroutineHelper.instance.RunCoroutine(_SendWebRequest(userServerUrl, data, onCompleted));
    }

    public void AlterPassword(string account, string oldPassword, string newPassword, Action<UnityWebRequest> onCompleted) {
      JObject json = JObject.FromObject(new { account, oldPassword, newPassword });
      json.Add("method", Methods.ALTERPSD.ToString());
      CoroutineHelper.instance.RunCoroutine(_SendWebRequest(userServerUrl, json, onCompleted));
    }

    private IEnumerator _SendWebRequest(string url, JObject data, Action<UnityWebRequest> onCompleted) {
      UnityWebRequest request = UnityWebRequest.Get(url);
      request.SetRequestHeader("data", StringUtil.StringToBase64(data.ToString()));
      request.downloadHandler = new DownloadHandlerBuffer();

      UIManager.instance.SetInteractionMask(true);
      yield return request.SendWebRequest();
      UIManager.instance.SetInteractionMask(false);

      onCompleted?.Invoke(request);
    }
  }
}