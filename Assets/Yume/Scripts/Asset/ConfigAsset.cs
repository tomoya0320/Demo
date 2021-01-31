using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System;

namespace Yume.Asset {
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

    public override Dictionary<(string, Type), object> GetActualData() {
      Dictionary<(string, Type), object> data = new Dictionary<(string, Type), object>();
      data.Add((nameof(userServerUrl), typeof(string)), userServerUrl);
      return data;
    }
  }
}