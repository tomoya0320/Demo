using UnityEngine;
using Newtonsoft.Json.Linq;
using Yume.Main;
using Newtonsoft.Json;

namespace Yume.Resource {
  public class AssetLoader : Singleton<AssetLoader> {
    private JObject m_data = new JObject();

    protected override void OnInit() {
      foreach (var url in Game.instance.assetUrls) {
        m_data.Merge(JObject.FromObject(ResourceManager.LoadAsset<Object>(url)));
      }
    }

    /// <summary>
    /// string int float等基本类型
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="name"></param>
    /// <returns></returns>
    public T GetValue<T>(string name) {
      if (m_data.ContainsKey(name)) {
        return m_data[name].Value<T>();
      }
      return default;
    }

    /// <summary>
    /// struct class Array等类型
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="name"></param>
    /// <returns></returns>
    public T GetObject<T>(string name) {
      if (m_data.ContainsKey(name)) {
        return JsonConvert.DeserializeObject<T>(m_data[name].ToString());
      }
      return default;
    }
  }
}