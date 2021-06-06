using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.UI;
using Yume.UI;

namespace Yume {
  public enum GlobalFadeType {
    BLACK_FADE,
    CIRCLE_FADE,
  }

  public static class UIManager {
    #region UIFolder

    public static Transform topCanvas { private set; get; }
    public static Transform tipCanvas { private set; get; }
    public static Transform cacheCanvas { private set; get; }
    public static Transform normalCanvas { private set; get; }
    public static Transform guidelCanvas { private set; get; }

    #endregion

    #region GlobalFade

    private static Animation s_blackFadeAnim;
    private static Animation s_circleFadeAnim;
    private static Image s_circleFadeImage;

    #endregion

    private static int s_blockNormalUI;
    public static int activeBack = 1;
    public static bool blockNormalUI => s_blockNormalUI > 0;
    public static bool globalFading { private set; get; }
    public static int uiCount => s_uiStack.Count;
    public static UIBase currentUI => s_uiStack.Count > 0 ? s_uiStack.Peek() : null;
    // 因为返回事件的添加和移除的顺序不是严格一致的所以不能用Stack之类的数据结构
    public static List<Action> onBackList = new List<Action>();
    private static Stack<UIBase> s_uiStack = new Stack<UIBase>();
    private static Dictionary<string, (float createTime, UIBase ui)> s_uiCacheDict = new Dictionary<string, (float, UIBase)>();

    public static void Init() {
      topCanvas = GameObject.Find("UIRoot/TopCanvas").transform;
      tipCanvas = GameObject.Find("UIRoot/TipCanvas").transform;
      cacheCanvas = GameObject.Find("UIRoot/CacheCanvas").transform;
      guidelCanvas = GameObject.Find("UIRoot/GuideCanvas").transform;
      normalCanvas = GameObject.Find("UIRoot/NormalCanvas").transform;

      GameObject tempGo = ObjectUtil.Instantiate("globalBlackFade", topCanvas);
      s_blackFadeAnim = tempGo.GetComponent<Animation>();
      tempGo.SetActiveEx(false);
      tempGo = ObjectUtil.Instantiate("globalCircleBlackFade", topCanvas);
      s_circleFadeAnim = tempGo.GetComponent<Animation>();
      s_circleFadeImage = tempGo.GetComponent<Image>();
      tempGo.SetActiveEx(false);

      onBackList.Add(
        () => {
          if (uiCount > 1 || currentUI?.name == "confirmation") {
            CloseOne(currentUI.name);
          } else {
            OpenOne("confirmation", null, "确认退出游戏？", "确认", "取消", new Action(Application.Quit), null);
          }
        }
      );

      Game.instance.onUpdate += () => {
        OnUpdate();
        if (Input.GetKeyDown(KeyCode.Escape)) {
          if (activeBack > 0) {
            onBackList.Last()?.Invoke();
          }
        }
      };
    }

    public static void OnUpdate() {
      _UpdateUI();
      _UpdateCache();
    }

    private static void _UpdateUI() {
      s_uiStack.ForEach(ui => ui.OnUpdate());
    }

    private static void _UpdateCache() {
      foreach (string name in s_uiCacheDict.Keys.ToArray()) {
        (float createTime, UIBase ui) = s_uiCacheDict[name];
        if (createTime + ui.cacheTime <= Time.realtimeSinceStartup) {
          s_uiCacheDict.Remove(name);
          ui.Destroy();
        }
      }
    }

    public static bool ContainUI(string name) {
      foreach (var ui in s_uiStack) {
        if (ui.name == name) {
          return true;
        }
      }
      return false;
    }

    /// <summary>
    /// 打开一个UI并入栈
    /// </summary>
    /// <param name="name"></param>
    /// <param name="callback"></param>
    /// <param name="args"></param>
    public static void OpenOne(string name, Action callback = null, params object[] args) {
      if (ResourceManager.Contains<UITemplate>(name)) {
        var data = ResourceManager.Load<UITemplate>(name);

        if (s_blockNormalUI > 0) {
          return;
        }

        _OpenInternal(data, callback, args);
      } else {
        Debug.LogError($"UI未加载！name：{name}");
      }
    }

    private static void _OpenInternal(UITemplate uiTemplate, Action callback = null, params object[] args) {
      if (!_TryGetCache(uiTemplate, out UIBase nextUI)) {
        nextUI = AssemblyUtil.CreateInstance<UIBase>(uiTemplate.typeName);
        GameObject tempGo = ObjectUtil.Instantiate(uiTemplate.prefabName, normalCanvas);
        tempGo.name = uiTemplate.name;
        nextUI.OnInit(uiTemplate, tempGo);
      }

      string lastBgm;
      string lastEffect;
      if (uiCount > 0) {
        lastBgm = currentUI.bgm;
        lastEffect = currentUI.effectName;
      } else {
        lastBgm = StringMap.EMPTY;
        lastEffect = StringMap.EMPTY;
      }

      s_uiStack.Push(nextUI);
      nextUI.Open(callback + GuideManager.CheckGuide, args);

      // 因为有些bgm和特效是在ui打开之后才会获取到的所以这里的顺序不能反了
      // 更新bgm和特效
      if (!nextUI.overrideBgm) {
        nextUI.bgm = lastBgm;
      }
      if (!nextUI.overrideEffect) {
        nextUI.effectName = lastEffect;
      }
      AudioManager.PlayBgm(nextUI.bgm);
      EffectManager.HideUIEffect(lastEffect);
      EffectManager.ShowUIEffect(nextUI.effectName);
    }

    /// <summary>
    /// 尝试从缓存中获取UI
    /// </summary>
    /// <param name="uiTemplate"></param>
    /// <param name="ui"></param>
    /// <returns></returns>
    private static bool _TryGetCache(UITemplate uiTemplate, out UIBase ui) {
      if (s_uiCacheDict.ContainsKey(uiTemplate.name)) {
        ui = s_uiCacheDict[uiTemplate.name].ui;
        ui.gameObject.SetParent(normalCanvas);
        s_uiCacheDict.Remove(uiTemplate.name);
        return true;
      }
      ui = null;
      return false;
    }

    /// <summary>
    /// 立即关闭栈中所有的UI并打开新的UI
    /// 相当于场景跳转
    /// </summary>
    /// <param name="name"></param>
    /// <param name="callback"></param>
    /// <param name="args"></param>
    public static void CloseAllAndOpenOne(string name, Action callback = null, params object[] args) {
      if (ResourceManager.Contains<UITemplate>(name)) {
        if (s_blockNormalUI > 0) {
          return;
        }

        // 当前UI不立即关闭
        if (uiCount > 0) {
          _CloseInternal(
            false, () => {
              while (uiCount > 0) {
                _CloseInternal(true, null, false);
              }
              OpenOne(name, callback, args);
            }
          );
        }
      } else {
        Debug.LogError($"UI未加载！name：{name}");
      }
    }

    /// <summary>
    /// 关闭栈顶UI
    /// </summary>
    /// <param name="name"></param>
    /// <param name="immediate">是否跳过关闭动画</param>
    /// <param name="callback"></param>
    public static void CloseOne(string name, bool immediate = false, Action callback = null) {
      if (ResourceManager.Contains<UITemplate>(name)) {
        if (s_blockNormalUI > 0) {
          return;
        }

        if (uiCount > 0 && currentUI.name == name) {
          _CloseInternal(immediate, callback);
        }
      } else {
        Debug.LogError($"UI未加载！name：{name}");
      }
    }

    /// <summary>
    /// 关闭该UI以及其之上的UI
    /// </summary>
    /// <param name="name"></param>
    /// <param name="immediate">是否跳过关闭动画</param>
    /// <param name="callback"></param>
    public static void CloseOneEx(string name, bool immediate = false, Action callback = null) {
      if (!ContainUI(name)) {
        Debug.LogError($"UI未打开！name:{name}");
        return;
      }

      while (currentUI.name != name) {
        _CloseInternal(true);
      }
      CloseOne(name, immediate, callback);
    }

    private static void _CloseInternal(bool immediate = false, Action callback = null, bool switchBgm = true) {
      // Action的执行顺序要特别当心
      // 先执行下面的再执行其他的回调
      // 因为其他的回调里可能会执行打开UI的操作
      Action action = () => {
        UIBase lastUI = s_uiStack.Pop();
        if (lastUI.cacheTime > 0) {
          lastUI.gameObject.SetParent(cacheCanvas);
          s_uiCacheDict.Add(lastUI.name, (Time.realtimeSinceStartup, lastUI));
        } else {
          lastUI.Destroy();
        }

        // 更新Bgm和特效
        string nextBgm;
        string nextEffect;
        if (uiCount > 0) {
          nextBgm = currentUI.bgm;
          nextEffect = currentUI.effectName;
        } else {
          nextBgm = StringMap.EMPTY;
          nextEffect = StringMap.EMPTY;
        }
        if (switchBgm) {
          AudioManager.PlayBgm(nextBgm);
        }
        EffectManager.HideUIEffect(lastUI.effectName);
        EffectManager.ShowUIEffect(nextEffect);
      };
      callback = action + callback;

      currentUI.Close(immediate, callback + GuideManager.CheckGuide);
    }

    public static void BlockNormalUI(UIBase ui) {
      s_blockNormalUI++;
      ui.canvasGroup.interactable = false;
    }

    public static void UnblockNormalUI(UIBase ui) {
      s_blockNormalUI--;
      ui.canvasGroup.interactable = true;
    }

    public static void GlobalFade(GlobalFadeType type, Action callback) {
      Animation animation = null;
      switch (type) {
        case GlobalFadeType.BLACK_FADE:
          animation = s_blackFadeAnim;
          break;
        case GlobalFadeType.CIRCLE_FADE:
          Vector2 clickPos = ScreenUtil.mousePosNormalized;
          Color color = s_circleFadeImage.color;
          color.r = clickPos.x;
          color.g = clickPos.y;
          s_circleFadeImage.color = color;
          animation = s_circleFadeAnim;
          break;
      }
      if (animation == null) {
        Debug.LogError($"未知的全局过渡类型！{type}");
        return;
      }
      CoroutineHelper.instance.Run(_GlobalFade(animation, callback));
    }

    private static IEnumerator _GlobalFade(Animation animation, Action callback) {
      activeBack--;
      globalFading = true;
      animation.gameObject.SetActiveEx(true);
      animation.Play("open");
      YWaitWhile waitWhile = WaitUtil.GetWaitWhile(() => animation.isPlaying);
      yield return waitWhile;
      callback?.Invoke();
      animation.Play("close");
      yield return waitWhile;
      WaitUtil.ReleaseWaitWhile(waitWhile);
      animation.gameObject.SetActiveEx(false);
      globalFading = false;
      activeBack++;
    }
  }
}