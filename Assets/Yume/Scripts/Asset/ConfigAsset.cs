using Yume.String;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;

namespace Yume.Setting {
  //因为ScriptableObject父类里有些其他不需要的字段所以需要反向剔除
  [JsonObject(MemberSerialization.OptIn)]
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

    [JsonProperty]
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