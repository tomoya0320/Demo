using System;
using System.Collections.Generic;
using Sirenix.Utilities;
using UnityEngine;
using Yume.UI;
using Object = UnityEngine.Object;

namespace Yume {
  public static class UIManager {
    public static Transform topCanvas { private set; get; }
    public static Transform tipCanvas { private set; get; }
    public static Transform cacheCanvas { private set; get; }
    public static Transform normalCanvas { private set; get; }
    public static Transform guidelCanvas { private set; get; }
    public static int activeBack = 1;
    // 因为返回事件的添加和移除的顺序不是严格一致的所以不能用Stack之类的数据结构
    public static List<Action> onBackList = new List<Action>();
    private static Dictionary<UIType, int> s_blockDict = new Dictionary<UIType, int>();
    private static Dictionary<UIType, Stack<UIBase>> s_uiStackDict = new Dictionary<UIType, Stack<UIBase>>();
    private static Dictionary<UIType, List<(float createTime, UIBase ui)>> s_uiCacheDict = new Dictionary<UIType, List<(float, UIBase)>>();

    public static void Init() {
      var root = GameObject.Find("UIRoot");
      topCanvas = root.FindGo("TopCanvas").transform;
      tipCanvas = root.FindGo("TipCanvas").transform;
      cacheCanvas = root.FindGo("CacheCanvas").transform;
      guidelCanvas = root.FindGo("GuideCanvas").transform;
      normalCanvas = root.FindGo("NormalCanvas").transform;

      foreach (UIType type in Enum.GetValues(typeof(UIType))) {
        s_blockDict.Add(type, 0);
        s_uiStackDict.Add(type, new Stack<UIBase>());
        s_uiCacheDict.Add(type, new List<(float, UIBase)>());
      }

      onBackList.Add(
        () => {
          var uiStack = s_uiStackDict[UIType.NORMAL];
          if (uiStack.Count > 1 || (uiStack.Count > 0 && uiStack.Peek().name == "confirmation")) {
            CloseOne(uiStack.Peek().name);
          } else {
            OpenOne("confirmation", null, "确认退出游戏？", "确认", "取消", new Action(Application.Quit), null);
          }
        }
      );
    }

    public static void OnUpdate() {
      _UpdateUI();
      _UpdateCache();
    }

    private static void _UpdateUI() {
      foreach (var uiStack in s_uiStackDict.Values) {
        uiStack.ForEach(ui => ui.OnUpdate());
      }
    }

    private static void _UpdateCache() {
      foreach (var list in s_uiCacheDict.Values) {
        for (int i = list.Count - 1; i >= 0; i--) {
          if (list[i].ui.cacheTime > 0 && list[i].createTime +
            list[i].ui.cacheTime <= Time.realtimeSinceStartup) {

            list[i].ui.Destroy();
            list.RemoveAt(i);
          }
        }
      }
    }

    public static UIBase GetTopUI(UIType uiType) {
      return s_uiStackDict[uiType].Peek();
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

        if (s_blockDict[data.uiType] > 0) {
          return;
        }

        var stack = s_uiStackDict[data.uiType];
        _OpenInternal(data, stack, callback, args);
      } else {
        Debug.LogError($"UI未加载！name：{name}");
      }
    }

    private static void _OpenInternal(UITemplate uiTemplate, Stack<UIBase> stack, Action callback = null, params object[] args) {
      if (!_TryGetCache(uiTemplate, out UIBase ui)) {
        ui = AssemblyUtil.CreateInstance(uiTemplate.typeName) as UIBase;
        GameObject prefab = ResourceManager.Load<GameObject>(uiTemplate.prefabName);
        if (prefab == null) {
          Debug.LogError($"Prefab为空！url:{uiTemplate.prefabName}");
          return;
        }
        ui.OnInit(uiTemplate, Object.Instantiate(prefab, _GetRootByType(uiTemplate.uiType)));
      }
      
      stack.Push(ui);
      ui.Open(callback, args);
    }

    /// <summary>
    /// 尝试从缓存中获取UI
    /// </summary>
    /// <param name="uiTemplate"></param>
    /// <param name="ui"></param>
    /// <returns></returns>
    private static bool _TryGetCache(UITemplate uiTemplate, out UIBase ui) {
      ui = null;
      
      int index = s_uiCacheDict[uiTemplate.uiType].
        FindIndex(tuple => tuple.ui.name == uiTemplate.name);
      
      if (index >= 0) {
        ui = s_uiCacheDict[uiTemplate.uiType][index].ui;
        ui.root.SetParent(_GetRootByType(uiTemplate.uiType));
        s_uiCacheDict[uiTemplate.uiType].RemoveAt(index);
        return true;
      }
      
      return false;
    }

    /// <summary>
    /// 销毁栈中的所有UI并打开新的UI
    /// 相当于场景跳转
    /// 关闭的UI不会被回收
    /// </summary>
    /// <param name="name"></param>
    /// <param name="callback"></param>
    /// <param name="args"></param>
    public static void DestroyAllAndOpenOne(string name, Action callback = null, params object[] args) {
      if (ResourceManager.Contains<UITemplate>(name)) {
        var data = ResourceManager.Load<UITemplate>(name);

        if (s_blockDict[data.uiType] > 0) {
          return;
        }

        var stack = s_uiStackDict[data.uiType];

        while (stack.Count > 0) {
          stack.Pop().Destroy();
        }
        OpenOne(name, callback, args);
      } else {
        Debug.LogError($"UI未加载！name：{name}");
      }
    }
    
    /// <summary>
    /// 立即销毁UI不播放close动画
    /// 关闭的UI不会被回收
    /// </summary>
    /// <param name="name"></param>
    public static void DestroyOne(string name) {
      if (ResourceManager.Contains<UITemplate>(name)) {
        var data = ResourceManager.Load<UITemplate>(name);

        if (s_blockDict[data.uiType] > 0) {
          return;
        }

        var stack = s_uiStackDict[data.uiType];

        if (stack.Count == 0) {
          Debug.LogError("UI栈为空！");
          return;
        }
        if (stack.Peek().name == name) {
          stack.Pop().Destroy();
        } else {
          Debug.LogError($"UI未在栈顶！name：{name}");
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
        var data = ResourceManager.Load<UITemplate>(name);

        if (s_blockDict[data.uiType] > 0) {
          return;
        }

        var stack = s_uiStackDict[data.uiType];

        if (stack.Count == 0) {
          Debug.LogError("UI栈为空！");
          return;
        }
        if (stack.Peek().name == name) {
          _CloseInternal(data, stack, callback);
        } else {
          Debug.LogError($"UI未在栈顶！name：{name}");
        }
      } else {
        Debug.LogError($"UI未加载！name：{name}");
      }
    }
    
    /// <summary>
    /// 回退操作只适用于NORMAL层的UI
    /// </summary>
    public static void Back() {
      if (activeBack > 0) {
        onBackList.Last()?.Invoke();
      }
    }

    private static void _CloseInternal(UITemplate uiTemplate, Stack<UIBase> stack, Action callback = null) {
      // Action的执行顺序要特别当心
      // 先执行下面的再执行其他的回调
      // 因为其他的回调里可能会执行打开UI的操作
      Action action = () => {
        // 回收
        UIBase ui = stack.Pop();
        ui.root.SetParent(cacheCanvas);
        s_uiCacheDict[uiTemplate.uiType].Add((Time.realtimeSinceStartup, ui));
      };
      callback = action + callback;

      stack.Peek().Close(callback);
    }

    private static Transform _GetRootByType(UIType uiType) {
      switch (uiType) {
        case UIType.NORMAL:
          return normalCanvas;
        case UIType.TOP:
          return topCanvas;
        default:
          return null;
      }
    }

    public static void Block(UIBase ui) {
      s_blockDict[ui.uiType]++;
      ui.canvasGroup.interactable = false;
    }

    public static void Unblock(UIBase ui) {
      s_blockDict[ui.uiType]--;
      ui.canvasGroup.interactable = true;
    }

    public static bool IsBlocked(UIType type) {
      return s_blockDict[type] > 0;
    }
  }
}