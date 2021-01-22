using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Yume.Data;
using Yume.Main;

namespace Yume.Resource {
  public class ResourceManager {
    #region Json
    private static JObject s_uiJson;
    private static JObject s_avgJson;
    private static JObject s_itemJson;
    private static JObject s_taskJson;
    private static JObject s_cardJson;
    private static JObject s_unitJson;
    private static JObject s_lvMaxApMapJson;
    #endregion

    private static Dictionary<(string, Type), object> s_resourceDict = new Dictionary<(string, Type), object>();

    public static IEnumerator Init() {
      yield return Addressables.InitializeAsync();

      var preloadRes = Addressables.LoadResourceLocationsAsync("Preload");
      yield return preloadRes;

      float process = 0;
      foreach (var location in preloadRes.Result) {
        object result;
        if (typeof(Sprite).IsAssignableFrom(location.ResourceType)) {
          var resource = Addressables.LoadAssetAsync<Sprite>(location);
          yield return resource;
          result = resource.Result;
        } else if (typeof(TextAsset).IsAssignableFrom(location.ResourceType)) {
          var resource = Addressables.LoadAssetAsync<TextAsset>(location);
          yield return resource;
          result = resource.Result;
        } else if (typeof(GameObject).IsAssignableFrom(location.ResourceType)) {
          var resource = Addressables.LoadAssetAsync<GameObject>(location);
          yield return resource;
          result = resource.Result;
        } else if (typeof(AudioClip).IsAssignableFrom(location.ResourceType)) {
          var resource = Addressables.LoadAssetAsync<AudioClip>(location);
          yield return resource;
          result = resource.Result;
        } else if (typeof(ScriptableObject).IsAssignableFrom(location.ResourceType)) {
          var resource = Addressables.LoadAssetAsync<ScriptableObject>(location);
          yield return resource;
          result = resource.Result;
        } else if (typeof(Texture2D).IsAssignableFrom(location.ResourceType)) {
          Debug.LogWarning($"无用的类型！Type:{location.ResourceType.FullName}");
          continue;
        } else {
          Debug.LogError($"未知的类型！Type:{location.ResourceType.FullName}");
          continue;
        }

        s_resourceDict.Add((location.PrimaryKey, location.ResourceType), result);
        Game.instance.initProcess = process++ / preloadRes.Result.Count;
      }
      s_uiJson = JObject.Parse(LoadAsset<TextAsset>("uiJson").text);
      s_avgJson = JObject.Parse(LoadAsset<TextAsset>("avgJson").text);
      s_itemJson = JObject.Parse(LoadAsset<TextAsset>("itemJson").text);
      s_taskJson = JObject.Parse(LoadAsset<TextAsset>("taskJson").text);
      s_cardJson = JObject.Parse(LoadAsset<TextAsset>("cardJson").text);
      s_unitJson = JObject.Parse(LoadAsset<TextAsset>("charJson").text);
      s_unitJson.Merge(JObject.Parse(LoadAsset<TextAsset>("enemyJson").text));
      s_lvMaxApMapJson = JObject.Parse(LoadAsset<TextAsset>("lvMaxApMapJson").text);

      yield return Resources.UnloadUnusedAssets();

      Game.instance.initProcess = 1.0f;
    }

    public static ItemTemplate GetItemTemplate(string id) {
      if (s_itemJson.ContainsKey(id)) {
        return JsonConvert.DeserializeObject<ItemTemplate>(s_itemJson[id].Value<JObject>().ToString());
      } else {
        throw new Exception(string.Format("item id:{0} is not found", id));
      }
    }

    public static CardTemplate GetCardTemplate(string id) {
      if (s_cardJson.ContainsKey(id)) {
        return JsonConvert.DeserializeObject<CardTemplate>(s_cardJson[id].Value<JObject>().ToString());
      } else {
        throw new Exception(string.Format("card id:{0} is not found", id));
      }
    }

    public static TaskTemplate GetTaskTemplate(TaskType taskType, string id) {
      if (s_taskJson.ContainsKey(taskType.ToString())) {
        JObject tempJObject = s_taskJson[taskType.ToString()].Value<JObject>();
        if (tempJObject.ContainsKey(id)) {
          return JsonConvert.DeserializeObject<TaskTemplate>(tempJObject[id].Value<JObject>().ToString());
        } else {
          throw new Exception(string.Format("task id:{0} is not found", id));
        }
      } else {
        throw new Exception(string.Format("task type:{0} is not found", taskType.ToString()));
      }
    }

    public static UnitTemplate GetUnitTemplate(string id) {
      if (s_unitJson.ContainsKey(id)) {
        return JsonConvert.DeserializeObject<UnitTemplate>(s_unitJson[id].Value<JObject>().ToString());
      }
      throw new Exception($"unit is not found! id:{id}");
    }

    public static IJEnumerable<JToken> GetTaskJsons(TaskType taskType) {
      if (s_taskJson.ContainsKey(taskType.ToString())) {
        return s_taskJson[taskType.ToString()].Values();
      }
      return default;
    }

    public static JArray GetAvgJson(string avgName) {
      if (s_avgJson.ContainsKey(avgName)) {
        return s_taskJson[avgName].Value<JArray>();
      }
      return default;
    }

    public static bool TryGetMaxAp(int level, out int maxAp) {
      maxAp = 0;
      if (s_lvMaxApMapJson.ContainsKey(level.ToString())) {
        maxAp = s_lvMaxApMapJson[level.ToString()].Value<int>();
        return true;
      }
      return false;
    }

    public static AnimationCurve GetAttribCurve(string id) {
      var attribCurveAsset = LoadAsset<AttribCurveAsset>("attribCurveAsset");
      int index = attribCurveAsset.attribCurves.FindIndex(ac => ac.id == id);
      if (index >= 0) {
        return attribCurveAsset.attribCurves[index].curve;
      } else {
        throw new Exception($"找不到属性曲线！id:{id}");
      }
    }

    public static IJEnumerable<JToken> GetAllUIJsons() {
      return s_uiJson.Values();
    }

    public static T LoadAsset<T>(string name) where T : class {
      if (s_resourceDict.ContainsKey((name, typeof(T)))) {
        return s_resourceDict[(name, typeof(T))] as T;
      }
      return default;
    }
  }
}