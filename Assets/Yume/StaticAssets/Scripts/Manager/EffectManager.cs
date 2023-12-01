using System.Collections.Generic;
using UnityEngine;
using Yume;
using Object = UnityEngine.Object;

public static class EffectManager {
  private class Effect {
    public GameObject gameObject;
    public int referenceCount;

    public Effect(GameObject gameObject) {
      this.gameObject = gameObject;
    }
  }
  
  private static Transform s_effectRoot;
  private static Dictionary<string, Effect> s_uiEffects = new Dictionary<string, Effect>();

  public static void Init() {
    s_effectRoot = GameObject.Find("EffectRoot").transform;
    
    // 空字符串特效占位用
    Effect emptyEffect = new Effect(null);
    emptyEffect.referenceCount++;
    s_uiEffects.Add(StringMap.EMPTY, emptyEffect);
  }

  public static void ShowUIEffect(string effectName) {
    if (!s_uiEffects.TryGetValue(effectName, out var effect)) {
      effect = new Effect(ObjectUtil.Instantiate(effectName, s_effectRoot));
      s_uiEffects.Add(effectName, effect);
    }

    effect.referenceCount++;
    effect.gameObject?.SetActiveEx(true);
  }

  public static void HideUIEffect(string effectName) {
    if (!s_uiEffects.TryGetValue(effectName, out var effect)) {
      Debug.LogError($"UI特效:{effectName}未添加！");
      return;
    }

    effect.referenceCount--;
    effect.gameObject?.SetActiveEx(false);
  }

  public static void DestroyUIEffect(string effectName) {
    if (!s_uiEffects.TryGetValue(effectName, out var effect)) {
      Debug.LogError($"UI特效:{effectName}未添加！");
      return;
    }

    if (effect.referenceCount == 0) {
      s_uiEffects.Remove(effectName);
      if (effect.gameObject != null) {
        Object.Destroy(effect.gameObject);
      }
    }
  }
}