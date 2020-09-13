using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;
using Yume.DB;
using Yume.Setting;
using Yume.Singleton;
using Yume.UI;

namespace Yume.Server {
  public class ServerController : SingletonMono<ServerController> {
    private string loginUrl {
      get {
        return Path.Combine(GlobalSettingHub.instance.serverUrl,
                            GlobalSettingHub.instance.loginServlet);
      }
    }
    private string registerUrl {
      get {
        return Path.Combine(GlobalSettingHub.instance.serverUrl,
                            GlobalSettingHub.instance.registerServlet);
      }
    }
    private string saveUrl {
      get {
        return Path.Combine(GlobalSettingHub.instance.serverUrl,
                            GlobalSettingHub.instance.saveServlet);
      }
    }
    private string alterPasswordUrl {
      get {
        return Path.Combine(GlobalSettingHub.instance.serverUrl,
                            GlobalSettingHub.instance.alterPasswordServlet);
      }
    }

    public void Login(string account, string password, Action<UnityWebRequest> onCompleted) {
      JObject json = ServerDataWrapper.Wrapper(new { account, password });
      StartCoroutine(_SendWebRequest(loginUrl, json, onCompleted));
    }

    public void Register(string account, string password, Action<UnityWebRequest> onCompleted) {
      //Java那边的Json有个bug，转数组时输入的Json字符串不能是空（""）
      //否则会出现类似"[null]"这样的结果，导致反序列化得到的List会变成Count为1且第一个元素为null
      var items = "[]";
      var tasks = DBManager.GetDefaultTasks();
      JObject json = ServerDataWrapper.Wrapper(new { account, password, items, tasks });
      StartCoroutine(_SendWebRequest(registerUrl, json, onCompleted));
    }

    public void Save<T>(T datas, Action<UnityWebRequest> onCompleted) where T : class {
      JObject json = ServerDataWrapper.Wrapper(datas);
      StartCoroutine(_SendWebRequest(saveUrl, json, onCompleted));
    }

    public void AlterPassword(string account, string oldPassword, string newPassword, Action<UnityWebRequest> onCompleted) {
      JObject json = ServerDataWrapper.Wrapper(new { account, oldPassword, newPassword });
      StartCoroutine(_SendWebRequest(alterPasswordUrl, json, onCompleted));
    }

    private IEnumerator _SendWebRequest(string url, JObject datas, Action<UnityWebRequest> onCompleted) {
      UnityWebRequest request = UnityWebRequest.Get(url);
      foreach (var key in datas.Properties()) {
        request.SetRequestHeader(key.Name, Regex.Replace(datas[key.Name].ToString(), @"\s", ""));
      }
      request.downloadHandler = new DownloadHandlerBuffer();

      UIController.instance.httpRequestWaitingMaskActive = true;
      yield return request.SendWebRequest();
      UIController.instance.httpRequestWaitingMaskActive = false;

      onCompleted?.Invoke(request);
    }
  }
}