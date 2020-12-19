using Yume.String;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;

namespace Yume.Setting {
  //因为ScriptableObject父类里有些其他不需要的字段所以需要反向剔除
  [JsonObject(MemberSerialization.OptIn)]
  public class ConfigAsset : ScriptableObject {
    public enum ServerUrlType {
      REMOTE, LOCAL
    }

    public ServerUrlType serverUrlType = ServerUrlType.LOCAL;
    public string remoteServerUrl;
    public string localServerUrl;
    public string userServlet;

    [JsonProperty]
    public string userServerUrl {
      get {
        switch (serverUrlType) {
          case ServerUrlType.LOCAL: {
              return Path.Combine(localServerUrl, userServlet);
            }
          case ServerUrlType.REMOTE: {
              return Path.Combine(remoteServerUrl, userServlet);
            }
          default: {
              return StringMap.EMPTY;
            }
        }
      }
    }
  }
}