using Yume.String;
using UnityEngine;

namespace Yume.Setting {
  [CreateAssetMenu]
  public class SettingAsset : ScriptableObject {
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
    private string _userServlet;
    public string userServlet => _userServlet;
  }
}