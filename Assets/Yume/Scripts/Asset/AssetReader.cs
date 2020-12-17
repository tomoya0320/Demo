using Yume.DB;
using Yume.Setting;

namespace Yume.Resource {
  public class AssetReader : Singleton<AssetReader> {
    private JsonAsset m_jsonAsset;
    public JsonAsset jsonAsset => m_jsonAsset;
    private SettingAsset m_settingAsset;
    public SettingAsset settingAsset => m_settingAsset;

    protected override void OnInit() {
      base.OnInit();
      m_jsonAsset = ResourceManager.LoadAsset<JsonAsset>("Asset/JsonAsset");
      m_settingAsset = ResourceManager.LoadAsset<SettingAsset>("Asset/settingAsset");
    }
  }
}