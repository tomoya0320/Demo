using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Yume.Asset;

namespace Yume.Resource {
  public class ResourceManager {
    private static Dictionary<(string key, Type type), object> s_resourceDict = new Dictionary<(string, Type), object>();

    public static IEnumerator Init(float min = 0, float max = 1.0f) {
      var locationList = Addressables.LoadResourceLocationsAsync("Preload");
      yield return locationList;

      float process = 0;
      foreach (var location in locationList.Result) {
        // 冗余的类型
        if (typeof(Texture2D).IsAssignableFrom(location.ResourceType)) {
          Debug.LogWarning($"冗余的类型:{location.ResourceType.FullName}");
        }
        // 特殊处理
        else if (typeof(Sprite).IsAssignableFrom(location.ResourceType)) {
          var sprite = Addressables.LoadAssetAsync<Sprite>(location);
          yield return sprite;

          s_resourceDict.Add((location.PrimaryKey, location.ResourceType), sprite.Result);
        } else if (typeof(YScriptableObject).IsAssignableFrom(location.ResourceType)) {
          var asset = Addressables.LoadAssetAsync<YScriptableObject>(location);
          yield return asset;

          foreach (var kv in asset.Result.GetActualData()) {
            s_resourceDict.Add(kv.Key, kv.Value);
          }
        }
        // 一般处理
        else {
          var res = Addressables.LoadAssetAsync<object>(location);
          yield return res;

          s_resourceDict.Add((location.PrimaryKey, location.ResourceType), res.Result);
        }

        Game.instance.process = min + (max - min) * (process++ / locationList.Result.Count);
      }
    }

    // 加载图片、音频、预制体等通用方法
    public static T Load<T>(string key) {
      if (string.IsNullOrEmpty(key)) {
        throw new Exception("key is null or empty!");
      }
      
      if (s_resourceDict.ContainsKey((key, typeof(T)))) {
        return (T)s_resourceDict[(key, typeof(T))];
      }
      return default;
    }

    public static T[] LoadAll<T>() {
      List<T> resources = new List<T>();
      foreach (var key in s_resourceDict.Keys) {
        if (key.type.IsAssignableFrom(typeof(T))) {
          resources.Add((T)s_resourceDict[key]);
        }
      }
      return resources.ToArray();
    }

    public static bool Contains<T>(string key) {
      if (string.IsNullOrEmpty(key)) {
        throw new Exception("key is null or empty!");
      }
      
      return s_resourceDict.ContainsKey((key, typeof(T)));
    }
  }
}