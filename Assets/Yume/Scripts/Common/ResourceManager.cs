using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Yume.Resource {
  public class ResourceManager { 
    /// <summary>
    /// 通过Resources加载
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="url"></param>
    /// <returns></returns>
    public static T LoadAsset<T>(string url) where T : Object {
      return Resources.Load<T>(url);
    }

    /// <summary>
    /// 通过Addressables加载(目前只能异步)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="url"></param>
    /// <param name="onComplete"></param>
    public static Task<T> LoadAssetAsync<T>(string url) {
      return Addressables.LoadAssetAsync<T>(url).Task;
    }
  }
}