using UnityEngine;
using Yume.Main;
using System.Collections.Generic;

namespace Yume.Resource {
  public abstract class YScriptableObject : ScriptableObject {
    public abstract Dictionary<string, object> GetAssets();
  }

  public class AssetLoader : Singleton<AssetLoader> {
    private Dictionary<string, object> m_assets = new Dictionary<string, object>();

    protected override void OnInit() {
      foreach (var asset in Game.instance.assets) {
        foreach (var kv in asset.GetAssets()) {
          m_assets.Add(kv.Key, kv.Value);
        }
      }
    }

    public T GetValue<T>(string name) {
      if (m_assets.ContainsKey(name)) {
        return (T)m_assets[name];
      }
      return default;
    }
  }
}