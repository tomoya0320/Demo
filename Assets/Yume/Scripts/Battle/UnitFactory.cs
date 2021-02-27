using UnityEngine;
using Yume.Data;
using Yume.Resource;

namespace Yume.Battle {
  public static class UnitFactory {
    public static Transform charFolder { private set; get; }
    public static Transform enemyFolder { private set; get; }

    public static void Init(ObjectContainer objectContainer) {
      charFolder = objectContainer.GetGameObject("charFolder").transform;
      enemyFolder = objectContainer.GetGameObject("enemyFolder").transform;
    }

    public static Unit CreateUnit<T>(UnitData unitData, CardData[] cardDatas) where T : Unit {
      Unit unit = null;

      if (typeof(Character).IsAssignableFrom(typeof(T))) {
        unit = new Character(unitData, cardDatas);
      } else if (typeof(Enemy).IsAssignableFrom(typeof(T))) {
        unit = new Enemy(unitData, cardDatas);
      }

      return unit;
    }

    public static void CleanUp() {
      charFolder = null;
      enemyFolder = null;
    }
  }
}