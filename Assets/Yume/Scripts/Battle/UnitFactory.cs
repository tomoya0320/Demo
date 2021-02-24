using UnityEngine;
using Yume.Data;
using Yume.Resource;

namespace Yume.Battle {
  public static class UnitFactory {
    private static Transform s_charFolder;
    private static Transform s_enemyFolder;
    
    public static void Init() {
      s_charFolder = GameObject.Find("charFolder").transform;
      s_enemyFolder = GameObject.Find("enemyFolder").transform;
    }

    public static GameObject CreateUnitGameObject(string prefabName, Camp camp) {
      var prefab = ResourceManager.Load<GameObject>(prefabName);
      Transform parent;
      switch (camp) {
        case Camp.ALLY:
          parent = s_charFolder;
          break;
        case Camp.ENEMY:
          parent = s_enemyFolder;
          break;
        default:
          parent = null;
          break;
      }
      return Object.Instantiate(prefab, parent);
    }
    
    public static void CleanUp() {
      s_charFolder = null;
      s_enemyFolder = null;
    }
  }
}