using UnityEngine;
using Yume.Singleton;

namespace Yume.DB {
  public class GlobalJsonHub : SingletonMono<GlobalJsonHub> {
    [SerializeField]
    private GlobalJson _jsonAsset;
    public GlobalJson jsonAsset => _jsonAsset;
  }
}