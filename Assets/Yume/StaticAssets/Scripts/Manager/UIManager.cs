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

    private static int s_block;
    public static int activeBack = 1;
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
          if (s_uiStack.Count > 1 || (s_uiStack.Count > 0 && s_uiStack.Peek().name == "confirmation")) {
            CloseOne(s_uiStack.Peek().name);
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
      foreach (var name in s_uiCacheDict.Keys.ToArray()) {
        (float createTime, UIBase ui) = s_uiCacheDict[name];
        if (ui.cacheTime > 0 && createTime + ui.cacheTime <= Time.realtimeSinceStartup) {
          s_uiCacheDict.Remove(name);
          ui.Destroy();
        }
      }
    }

    public static UIBase GetTopUI() {
      return s_uiStack.Peek();
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

        if (s_block > 0) {
          return;
        }

        _OpenInternal(data, callback, args);
      } else {
        Debug.LogError($"UI未加载！name：{name}");
      }
    }

    private static void _OpenInternal(UITemplate uiTemplate, Action callback = null, params object[] args) {
      if (!_TryGetCache(uiTemplate, out UIBase ui)) {
        ui = AssemblyUtil.CreateInstance<UIBase>(uiTemplate.typeName);
        GameObject tempGo = ObjectUtil.Instantiate(uiTemplate.prefabName, normalCanvas);
        tempGo.name = uiTemplate.name;
        ui.OnInit(uiTemplate, tempGo);
      }

      if (s_uiStack.Count > 0) {
        s_uiStack.Peek().OnHide();
      }
      s_uiStack.Push(ui);
      ui.Open(callback, args);
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
    /// 销毁栈中所有的UI并打开新的UI
    /// 相当于场景跳转
    /// 关闭的UI不会被回收
    /// </summary>
    /// <param name="name"></param>
    /// <param name="callback"></param>
    /// <param name="args"></param>
    public static void DestroyAllAndOpenOne(string name, Action callback = null, params object[] args) {
      if (ResourceManager.Contains<UITemplate>(name)) {
        if (s_block > 0) {
          return;
        }

        while (s_uiStack.Count > 0) {
          s_uiStack.Pop().Destroy();
        }
        OpenOne(name, callback, args);
      } else {
        Debug.LogError($"UI未加载！name：{name}");
      }
    }

    /// <summary>
    /// 销毁栈中当前的UI并打开新的UI
    /// 关闭的UI不会被回收
    /// </summary>
    /// <param name="name"></param>
    /// <param name="callback"></param>
    /// <param name="args"></param>
    public static void DestroyOneAndOpenOne(string name, Action callback = null, params object[] args) {
      if (ResourceManager.Contains<UITemplate>(name)) {
        if (s_block > 0) {
          return;
        }

        if (s_uiStack.Count > 0) {
          s_uiStack.Pop().Destroy();
        }
        OpenOne(name, callback, args);
      } else {
        Debug.LogError($"UI未加载！name：{name}");
      }
    }

    /// <summary>
    /// 销毁栈中当前的UI
    /// 关闭的UI不会被回收
    /// </summary>
    /// <param name="name"></param>
    public static void DestroyOne(string name) {
      if (ResourceManager.Contains<UITemplate>(name)) {
        if (s_block > 0) {
          return;
        }

        if (s_uiStack.Count > 0 && s_uiStack.Peek().name == name) {
          s_uiStack.Pop().Destroy();
        }
      } else {
        Debug.LogError($"UI未加载！name：{name}");
      }
    }

    /// <summary>
    /// 关闭栈顶UI
    /// </summary>
    /// <param name="name"></param>
    /// <param name="callback"></param>
    public static void CloseOne(string name, Action callback = null) {
      if (ResourceManager.Contains<UITemplate>(name)) {
        if (s_block > 0) {
          return;
        }

        if (s_uiStack.Count == 0) {
          Debug.LogError("UI栈为空！");
          return;
        }
        if (s_uiStack.Peek().name == name) {
          _CloseInternal(callback);
        } else {
          Debug.LogError($"UI未在栈顶！name：{name}");
        }
      } else {
        Debug.LogError($"UI未加载！name：{name}");
      }
    }

    private static void _CloseInternal(Action callback = null) {
      // Action的执行顺序要特别当心
      // 先执行下面的再执行其他的回调
      // 因为其他的回调里可能会执行打开UI的操作
      Action action = () => {
        UIBase ui = s_uiStack.Pop();
        ui.gameObject.SetParent(cacheCanvas);
        s_uiCacheDict.Add(ui.name, (Time.realtimeSinceStartup, ui));

        if (s_uiStack.Count > 0) {
          s_uiStack.Peek().OnDisplay();
        }
      };
      callback = action + callback;

      s_uiStack.Peek().Close(callback);
    }

    public static void Block(UIBase ui) {
      s_block++;
      ui.canvasGroup.interactable = false;
    }

    public static void Unblock(UIBase ui) {
      s_block--;
      ui.canvasGroup.interactable = true;
    }

    public static bool IsAnyBlocked() {
      return s_block > 0;
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
      s_block++;
      animation.gameObject.SetActiveEx(true);
      animation.Play("open");
      yield return new WaitWhile(() => animation.isPlaying);
      s_block--;
      
      callback?.Invoke();
      
      s_block++;
      animation.Play("close");
      yield return new WaitWhile(() => animation.isPlaying);
      animation.gameObject.SetActiveEx(false);
      s_block--;
    }
  }
}