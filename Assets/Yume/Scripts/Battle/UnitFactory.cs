using UnityEngine;
using Yume.Data;
using Yume.Resource;

namespace Yume.Battle {
  public static class UnitFactory {
    public static Transform charFolder { private set; get; }
    public static Transform enemyFolder { private set; get; }

    public static void Init(ObjectContainer objectContainer) {
      charFolder = objectContainer.GetTransform("charFolder");
      enemyFolder = objectContainer.GetTransform("enemyFolder");
    }

    public static Unit CreateUnit<T>(UnitArgs unitArgs, CardArgs[] cardArgsArray) where T : Unit {
      Unit unit = null;

      if (typeof(Character).IsAssignableFrom(typeof(T))) {
        unit = new Character(unitArgs, cardArgsArray);
      } else if (typeof(Enemy).IsAssignableFrom(typeof(T))) {
        unit = new Enemy(unitArgs, cardArgsArray);
      }

      return unit;
    }

    public static void CleanUp() {
      charFolder = null;
      enemyFolder = null;
    }
  }
}