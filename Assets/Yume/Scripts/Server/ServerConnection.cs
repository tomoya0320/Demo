using System;
using System.Collections;
using System.IO;
using UnityEngine.Networking;
using Yume.Setting;
using Yume.Singleton;
using Yume.UI;

namespace Yume.Server {
  public class ServerConnection : SingletonMono<ServerConnection> {
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

    public void Login(string account, string password, Action<UnityWebRequest> action) {
      StartCoroutine(_SendWebRequest(loginUrl, account, password, action));
    }   
    
    public void Register(string account, string password, Action<UnityWebRequest> action) {
      StartCoroutine(_SendWebRequest(registerUrl, account, password, action));
    }

    private IEnumerator _SendWebRequest(string url, string account, string password, Action<UnityWebRequest> action) {
      var request = UnityWebRequest.Get(url);
      request.SetRequestHeader("account", account);
      request.SetRequestHeader("password", password);
      request.downloadHandler = new DownloadHandlerBuffer();

      UIController.instance.httpRequestWaitingMaskActive = true;
      yield return request.SendWebRequest();
      UIController.instance.httpRequestWaitingMaskActive = false;

      action?.Invoke(request);
    }
  }
}