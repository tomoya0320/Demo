using UnityEngine;
using Newtonsoft.Json.Linq;
using Yume.Main;

namespace Yume.Resource {
  /// <summary>
  /// 只支持值类型的序列化
  /// TODO:添加UnityEngine类型的支持
  /// </summary>
  public class AssetLoader : Singleton<AssetLoader> {
    private JObject m_data = new JObject();

    protected override void OnBeforeInit() {
      foreach (var url in Game.instance.assetUrls) {
        m_data.Merge(JObject.FromObject(ResourceManager.LoadAsset<Object>(url)));
      }
    }

    public T GetValue<T>(string name) {
      if (m_data.ContainsKey(name)) {
        return m_data[name].Value<T>();
      }
      return default;
    }
  }
}