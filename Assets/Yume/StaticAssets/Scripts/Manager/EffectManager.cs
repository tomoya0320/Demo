using System.Collections.Generic;
using UnityEngine;
using Yume;
using Object = UnityEngine.Object;

public static class EffectManager {
  private static Transform s_effectRoot;
  private static GameObject s_curUIEffect;
  private static List<GameObject> s_uiEffects = new List<GameObject>();

  public static void Init() {
    s_effectRoot = GameObject.Find("EffectRoot").transform;
  }

  public static void AddUIEffect(string effectName) {
    GameObject effect = s_uiEffects.Find(go => go.name == effectName);
    if (effect != null) {
      Debug.LogError($"UI特效[{effectName}]已添加！");
      return;
    }
    
    effect = ObjectUtil.Instantiate(effectName, s_effectRoot);
    effect.SetActiveEx(false);

    s_uiEffects.Add(effect);
  }

  public static void RemoveUIEffect(string effectName) {
    GameObject effect = s_uiEffects.Find(go => go.name == effectName);
    if (effect == null) {
      Debug.LogError($"UI特效[{effectName}]未添加！");
      return;
    }
    
    s_uiEffects.Remove(effect);
    Object.Destroy(effect);
  }

  public static void ShowUIEffect(string effectName) {
    GameObject effect = s_uiEffects.Find(go => go.name == effectName);
    s_curUIEffect?.SetActiveEx(false);
    s_curUIEffect = effect;
    s_curUIEffect?.SetActiveEx(true);
  }

  public static void HideUIEffect() {
    s_curUIEffect?.SetActiveEx(false);
  }

  public static string GetCurUIEffect() {
    return s_curUIEffect?.name;
  }
}