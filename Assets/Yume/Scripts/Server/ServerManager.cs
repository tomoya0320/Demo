using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using Yume.DB;
using Yume.Resource;
using Yume.String;

namespace Yume.Server {
  public class ServerManager : SingletonMono<ServerManager> {
    private enum Methods {
      LOGIN, REGISTER, ALTERPSD, UPDATE
    }

    private string userUrl {
      get {
        return Path.Combine(AssetReader.instance.settingAsset.serverUrl,
                           AssetReader.instance.settingAsset.userServlet);
      }
    }
  
    public void Login(string account, string password, Action<UnityWebRequest> onCompleted) {
      JObject json = ServerDataWrapper.Wrapper(new { account, password });
      json.Add("method", Methods.LOGIN.ToString());
      //添加token以验证同步
      json.Add("token", DateTime.Now.ToString() + SystemInfo.deviceUniqueIdentifier);
      StartCoroutine(_SendWebRequest(userUrl, json, onCompleted));
    }

    public void Register(string name, string account, string password, Action<UnityWebRequest> onCompleted) {
      //Java那边的Json有个bug，转数组时输入的Json字符串不能是空（""）
      //否则会出现类似"[null]"这样的结果，导致反序列化得到的List会变成Count为1且第一个元素为null
      //update：换了一种方式初始化(直接new UserDB())
      JObject json = ServerDataWrapper.Wrapper(new { userDB = new UserDB(name/*暂时测试用*/, account, password) });
      json.Add("method", Methods.REGISTER.ToString());
      StartCoroutine(_SendWebRequest(userUrl, json, onCompleted));
    }

    public void UpdateUserData<T>(T data, Action<UnityWebRequest> onCompleted) where T : class {
      JObject json = ServerDataWrapper.Wrapper(data);
      json.Add("method", Methods.UPDATE.ToString());
      //用户账号和密码信息
      json.Add("account", DBManager.userAccount);
      json.Add("password", DBManager.userPassword);
      //添加token以验证同步
      json.Add("token", DBManager.userToken);
      StartCoroutine(_SendWebRequest(userUrl, json, onCompleted));
    }

    public void AlterPassword(string account, string oldPassword, string newPassword, Action<UnityWebRequest> onCompleted) {
      JObject json = ServerDataWrapper.Wrapper(new { account, oldPassword, newPassword });
      json.Add("method", Methods.ALTERPSD.ToString());
      StartCoroutine(_SendWebRequest(userUrl, json, onCompleted));
    }

    private IEnumerator _SendWebRequest(string url, JObject data, Action<UnityWebRequest> onCompleted) {
      UnityWebRequest request = UnityWebRequest.Get(url);
      request.SetRequestHeader("data", StringUtil.StringToBase64(data.ToString()));
      request.downloadHandler = new DownloadHandlerBuffer();

      //TODO:遮罩
      yield return request.SendWebRequest();
      //TODO:遮罩

      onCompleted?.Invoke(request);
    }
  }
}