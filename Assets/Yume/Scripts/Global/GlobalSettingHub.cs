using UnityEngine;
using Yume.Singleton;

namespace Yume.Setting {
  public class GlobalSettingHub : SingletonMono<GlobalSettingHub> {
    [SerializeField]
    private GlobalSetting _setting;
    public GlobalSetting setting => _setting;
  }
}