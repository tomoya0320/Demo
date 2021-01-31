using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Yume.Data;
using Yume.Main;
using Yume.Setting;
using Yume.UI;

namespace Yume.Resource {
  public class ResourceManager {
    private static Dictionary<(string key, Type type), object> s_resourceDict = new Dictionary<(string, Type), object>();

    public static IEnumerator Init() {
      yield return Addressables.InitializeAsync();

      #region 加载Json 这个比较特殊需要转换一下
      var textAsset = Addressables.LoadAssetAsync<TextAsset>("lvMaxApMapJson");
      yield return textAsset;

      JObject json = JObject.Parse(textAsset.Result.text);
      foreach (var kv in json) {
        s_resourceDict.Add((kv.Key, typeof(int)), kv.Value.Value<int>());
      }

      textAsset = Addressables.LoadAssetAsync<TextAsset>("uiJson");
      yield return textAsset;

      json = JObject.Parse(textAsset.Result.text);
      foreach (var kv in json) {
        UITemplate ui = JsonConvert.DeserializeObject<UITemplate>(kv.Value.ToString());
        s_resourceDict.Add((kv.Key, typeof(UITemplate)), ui);
      }

      textAsset = Addressables.LoadAssetAsync<TextAsset>("itemJson");
      yield return textAsset;

      json = JObject.Parse(textAsset.Result.text);
      foreach (var kv in json) {
        ItemTemplate itemTemplate = JsonConvert.DeserializeObject<ItemTemplate>(kv.Value.ToString());
        s_resourceDict.Add((kv.Key, typeof(ItemTemplate)), itemTemplate);
      }

      textAsset = Addressables.LoadAssetAsync<TextAsset>("taskJson");
      yield return textAsset;

      json = JObject.Parse(textAsset.Result.text);
      foreach (var kv in json) {
        // 初始的任务数据
        TaskData task = JsonConvert.DeserializeObject<TaskData>(kv.Value.ToString());
        s_resourceDict.Add((kv.Key, typeof(TaskData)), task);

        TaskTemplate taskTemplate = JsonConvert.DeserializeObject<TaskTemplate>(kv.Value.ToString());
        s_resourceDict.Add((kv.Key, typeof(TaskTemplate)), taskTemplate);
      }

      textAsset = Addressables.LoadAssetAsync<TextAsset>("cardJson");
      yield return textAsset;

      json = JObject.Parse(textAsset.Result.text);
      foreach (var kv in json) {
        CardTemplate cardTemplate = JsonConvert.DeserializeObject<CardTemplate>(kv.Value.ToString());
        s_resourceDict.Add((kv.Key, typeof(CardTemplate)), cardTemplate);
      }

      textAsset = Addressables.LoadAssetAsync<TextAsset>("unitJson");
      yield return textAsset;

      json = JObject.Parse(textAsset.Result.text);
      foreach (var kv in json) {
        UnitTemplate unitTemplate = JsonConvert.DeserializeObject<UnitTemplate>(kv.Value.ToString());
        s_resourceDict.Add((kv.Key, typeof(UnitTemplate)), unitTemplate);
      }
      #endregion

      Game.instance.initProcess = 0.1f;

      #region 加载ScriptableObject 这个也要转换一下
      var attribCurveAsset = Addressables.LoadAssetAsync<AttribCurveAsset>("attribCurveAsset");
      yield return attribCurveAsset;

      foreach (var attribCurve in attribCurveAsset.Result.attribCurves) {
        s_resourceDict.Add((attribCurve.id, typeof(AnimationCurve)), attribCurve.curve);
      }

      var configAsset = Addressables.LoadAssetAsync<ConfigAsset>("configAsset");
      yield return configAsset;

      s_resourceDict.Add((nameof(configAsset.Result.userServerUrl), typeof(string)), configAsset.Result.userServerUrl);
      #endregion

      Game.instance.initProcess = 0.1f;

      #region 其他预加载的资源可以直接加载
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
        }
        // 一般处理
        else {
          var res = Addressables.LoadAssetAsync<object>(location);
          yield return res;

          s_resourceDict.Add((location.PrimaryKey, location.ResourceType), res.Result);
        }

        Game.instance.initProcess = 0.1f + 0.8f * (process++ / locationList.Result.Count);
      }
      #endregion

      Game.instance.initProcess = 0.9f;

      #region 回收内存
      yield return Resources.UnloadUnusedAssets();

      GC.Collect();
      #endregion
    }

    // 加载图片、音频、预制体等通用方法
    public static T Load<T>(string key) {
      if (s_resourceDict.ContainsKey((key, typeof(T)))) {
        return (T)s_resourceDict[(key, typeof(T))];
      }
      return default;
    }

    public static T[] LoadAll<T>() {
      List<T> resList = new List<T>();
      foreach (var key in s_resourceDict.Keys) {
        if (key.type.IsAssignableFrom(typeof(T))) {
          resList.Add((T)s_resourceDict[key]);
        }
      }
      return resList.ToArray();
    }

    public static bool Contains<T>(string key) {
      return s_resourceDict.ContainsKey((key, typeof(T)));
    }
  }
}