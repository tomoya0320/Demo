using UnityEngine;
using System.IO;
using Yume.Resource;
using System.Collections.Generic;

namespace Yume.Setting {
  public class ConfigAsset : YScriptableObject {
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

    public override Dictionary<string, object> GetAssets() {
      Dictionary<string, object> assets = new Dictionary<string, object>();
      assets.Add(nameof(userServerUrl), userServerUrl);

      return assets;
    }
  }
}