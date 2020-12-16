using Yume.DB;
using Yume.Setting;
using Yume.Singleton;

namespace Yume.Resource {
  public class AssetReader:SingletonNoMono<AssetReader> {
    private JsonAsset m_jsonAsset;
    public JsonAsset jsonAsset => m_jsonAsset;
    private SettingAsset m_settingAsset;
    public SettingAsset settingAsset => m_settingAsset;

    protected override void OnInit() {
      base.OnInit();
      m_jsonAsset = ResourceManager.LoadAsset<JsonAsset>("JsonAsset");
      m_settingAsset = ResourceManager.LoadAsset<SettingAsset>("settingAsset");
    }
  }
}