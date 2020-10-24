using UnityEngine;
using Yume.DB;
using Yume.Singleton;

public class GlobalJsonHub : SingletonMono<GlobalJsonHub> {
  [SerializeField]
  private GlobalJson _jsonAsset;
  public GlobalJson jsonAsset => _jsonAsset;
}