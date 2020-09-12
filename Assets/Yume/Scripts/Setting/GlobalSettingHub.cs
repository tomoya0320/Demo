using UnityEngine;
using Yume.Singleton;
using Yume.String;

namespace Yume.Setting {
  public class GlobalSettingHub : SingletonMono<GlobalSettingHub> {
    private enum ServerUrlType {
      REMOTE, LOCAL
    }

    [SerializeField]
    private ServerUrlType _serverUrlType = ServerUrlType.LOCAL;
    [SerializeField]
    private string _remoteServerUrl;
    [SerializeField]
    private string _localServerUrl;
    public string serverUrl {
      get {
        switch (_serverUrlType) {
          case ServerUrlType.LOCAL: {
              return _localServerUrl;
            }
          case ServerUrlType.REMOTE: {
              return _remoteServerUrl;
            }
          default: {
              return StringMap.EMPTY;
            }
        }
      }
    }
    [SerializeField]
    private string _loginServlet;
    public string loginServlet => _loginServlet;
    [SerializeField]
    private string _registerServlet;
    public string registerServlet => _registerServlet;   
    [SerializeField]
    private string _saveServlet;
    public string saveServlet => _saveServlet;    
    [SerializeField]
    private string _alterPasswordServlet;
    public string alterPasswordServlet => _alterPasswordServlet;
  }
}