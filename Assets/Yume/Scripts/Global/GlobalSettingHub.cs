using UnityEngine;
using Yume.Singleton;

namespace Yume.Setting {
  public class GlobalSettingHub : SingletonMono<GlobalSettingHub> {
    [SerializeField]
    private GlobalSetting _settingAsset;
    public GlobalSetting settingAsset => _settingAsset;
  }
}