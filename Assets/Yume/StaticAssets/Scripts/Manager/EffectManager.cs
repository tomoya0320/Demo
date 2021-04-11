using System;
using System.Collections.Generic;
using UnityEngine;
using Yume;
using Object = UnityEngine.Object;

public static class EffectManager {
  private static Transform s_effectRoot;
  private static Dictionary<UIType, GameObject> s_curUIEffectDict = new Dictionary<UIType, GameObject>();
  private static Dictionary<UIType, Dictionary<string, GameObject>> s_uiEffectDicts = new Dictionary<UIType, Dictionary<string, GameObject>>();

  public static void Init() {
    s_effectRoot = GameObject.Find("EffectRoot").transform;

    foreach (UIType uiType in Enum.GetValues(typeof(UIType))) {
      s_curUIEffectDict.Add(uiType, null);
      s_uiEffectDicts.Add(uiType, new Dictionary<string, GameObject>());
    }
  }

  public static void AddUIEffect(UIType uiType, string effectName) {
    if (s_uiEffectDicts[uiType].ContainsKey(effectName)) {
      Debug.LogError($"UI特效[{effectName}]已添加！");
      return;
    }

    GameObject prefab = ResourceManager.Load<GameObject>(effectName);
    GameObject effect = Object.Instantiate(prefab, s_effectRoot);
    effect.name = effectName;
    effect.SetActiveEx(false);

    s_uiEffectDicts[uiType].Add(effectName, effect);
  }

  public static void RemoveUIEffect(UIType uiType, string effectName) {
    if (!s_uiEffectDicts[uiType].ContainsKey(effectName)) {
      Debug.LogError($"UI特效[{effectName}]未添加！");
      return;
    }

    Object.Destroy(s_uiEffectDicts[uiType][effectName]);
    s_uiEffectDicts[uiType].Remove(effectName);
  }

  public static void ShowUIEffect(UIType uiType, string effectName) {
    GameObject nextEffect = null;

    if (s_uiEffectDicts[uiType].ContainsKey(effectName)) {
      nextEffect = s_uiEffectDicts[uiType][effectName];
    }

    s_curUIEffectDict[uiType]?.SetActiveEx(false);
    s_curUIEffectDict[uiType] = nextEffect;
    s_curUIEffectDict[uiType]?.SetActiveEx(true);
  }
}