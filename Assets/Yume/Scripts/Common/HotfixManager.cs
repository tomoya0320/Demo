using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Yume.Hotfix {
  public static class HotfixManager {
    public static IEnumerator Init(float min = 0, float max = 1.0f) {
      var locationList = Addressables.LoadResourceLocationsAsync("Hotfix");
      yield return locationList;

      float process = 0;
      foreach (var location in locationList.Result) {
        var luaRes = Addressables.LoadAssetAsync<TextAsset>(location);
        yield return luaRes;

        XLuaUtil.DoString(luaRes.Result.text);

        Game.instance.initProcess = min + (max - min) * (process++ / locationList.Result.Count);
      }
    }
  }
}