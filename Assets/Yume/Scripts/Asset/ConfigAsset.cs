using UnityEngine;
using System.IO;

namespace Yume.Setting {
  public class ConfigAsset : ScriptableObject {
    private enum ServerUrlType {
      REMOTE, LOCAL
    }

    [SerializeField]
    private ServerUrlType _serverUrlType;
    [SerializeField]
    private string _remoteServerUrl;
    [SerializeField]
    private string _localServerUrl;
    [SerializeField]
    private string _userServlet;

    public string userServerUrl {
      get {
        switch (_serverUrlType) {
          case ServerUrlType.LOCAL: {
              return Path.Combine(_localServerUrl, _userServlet);
            }
          case ServerUrlType.REMOTE: {
              return Path.Combine(_remoteServerUrl, _userServlet);
            }
          default: {
              return StringMap.EMPTY;
            }
        }
      }
    }
  }
}