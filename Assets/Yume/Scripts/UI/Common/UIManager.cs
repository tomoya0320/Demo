using System;
using System.Collections.Generic;
using UnityEngine;
using Yume.Data;
using Yume.Main;
using Yume.Resource;

namespace Yume.UI {
  public static class UIManager {
    private static GameObject s_interactionMask;
    public static Transform topCanvas { private set; get; }
    public static Transform tipCanvas { private set; get; }
    public static Transform cacheCanvas { private set; get; }
    public static Transform normalCanvas { private set; get; }
    private static Dictionary<object, object> s_sharedArgsDict = new Dictionary<object, object>();
    private static Dictionary<UIType, Stack<UIBase>> s_uiStackDict = new Dictionary<UIType, Stack<UIBase>>();
    private static Dictionary<UIType, List<(float, UIBase)>> s_cacheUIListDict = new Dictionary<UIType, List<(float, UIBase)>>();
    private static Dictionary<UIType, int> s_blockDict = new Dictionary<UIType, int>();
    public static Stack<Action> onBackStack = new Stack<Action>();

    public static void Init() {
      ObjectContainer uiRootObject = Game.instance.uiRootObject;
      s_interactionMask = uiRootObject.GetGameObject("InteractMask");
      topCanvas = uiRootObject.GetGameObject("TopCanvas")?.transform;
      tipCanvas = uiRootObject.GetGameObject("TipCanvas")?.transform;
      cacheCanvas = uiRootObject.GetGameObject("CacheCanvas")?.transform;
      normalCanvas = uiRootObject.GetGameObject("NormalCanvas")?.transform;

      foreach (UIType type in Enum.GetValues(typeof(UIType))) {
        s_blockDict.Add(type, 0);
        s_uiStackDict.Add(type, new Stack<UIBase>());
        s_cacheUIListDict.Add(type, new List<(float, UIBase)>());
      }

      onBackStack.Push(() => {
        if (s_uiStackDict[UIType.NORMAL].Count > 1) {
          CloseOne(s_uiStackDict[UIType.NORMAL].Peek().name);
        } else {
          OpenOne("confirmation", null, "确认退出游戏？", "确认", "取消", new Action(Application.Quit), null);
        }
      });
    }

    public static void OnUpdate() {
      _UpdateUI();
      _UpdateCache();
    }

    private static void _UpdateUI() {
      foreach (var uiStack in s_uiStackDict.Values) {
        foreach (var ui in uiStack) {
          ui.OnUpdate();
        }
      }
    }

    private static void _UpdateCache() {
      foreach (var list in s_cacheUIListDict.Values) {
        for (int i = list.Count - 1; i >= 0; i--) {
          if (list[i].Item2.cacheTime > 0 && list[i].Item1 +
              list[i].Item2.cacheTime <= Time.realtimeSinceStartup) {

            list[i].Item2.Destroy();
            list.RemoveAt(i);
          }
        }
      }
    }

    public static void SetSharedArgs(object key, object value) {
      if (s_sharedArgsDict.ContainsKey(key)) {
        s_sharedArgsDict[key] = value;
      } else {
        s_sharedArgsDict.Add(key, value);
      }
    }

    public static T GetSharedArgs<T>(object key) {
      if (s_sharedArgsDict.ContainsKey(key)) {
        return (T)s_sharedArgsDict[key];
      } else {
        return default;
      }
    }

    public static UIBase GetTopUI(UIType uiType) {
      return s_uiStackDict[uiType].Peek();
    }

    /// <summary>
    /// 打开一个UI并入栈同时选择性关闭上一个UI
    /// </summary>
    /// <param name="name"></param>
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

    private static void _OpenInternal(UITemplate data, Stack<UIBase> stack, Action callback = null, params object[] args) {
      UIBase ui = null;
      if (!_TryGetCache(data, ref ui)) {
        ui = AssemblyTool.CreateInstance(data.typeName) as UIBase;
        GameObject prefab = ResourceManager.Load<GameObject>(data.prefabName);
        if (prefab == null) {
          Debug.LogError($"Prefab为空！url:{data.prefabName}");
          return;
        }
        ui.OnInit(data, UnityEngine.Object.Instantiate(prefab, _GetRootByType(data.uiType)));
      }
      stack.Push(ui);
      ui.Open(callback, args);
    }

    /// <summary>
    /// 尝试从缓存中获取UI
    /// </summary>
    /// <param name="uiTemplate"></param>
    /// <returns></returns>
    private static bool _TryGetCache(UITemplate uiTemplate, ref UIBase ui) {
      var index = s_cacheUIListDict[uiTemplate.uiType].FindIndex(i => i.Item2.name == uiTemplate.name);
      if (index >= 0) {
        ui = s_cacheUIListDict[uiTemplate.uiType][index].Item2;
        s_cacheUIListDict[uiTemplate.uiType].RemoveAt(index);
        ui.root.transform.SetParent(_GetRootByType(uiTemplate.uiType));
        return true;
      }
      return false;
    }

    /// <summary>
    /// 销毁栈中的所有UI并打开新的UI，相当于场景跳转
    /// </summary>
    /// <param name="name"></param>
    /// <param name="args"></param>
    public static void CloseAllAndOpenOne(string name, Action callback = null, params object[] args) {
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
    /// 关闭栈顶UI并选择性打开上一个UI
    /// </summary>
    /// <param name="name"></param>
    /// <param name="args"></param>
    public static void CloseOne(string name, Action callback = null, params object[] args) {
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
          _CloseInternal(data, stack, callback, args);
        } else {
          Debug.LogError($"UI未在栈顶！name：{name}");
        }
      } else {
        Debug.LogError($"UI未加载！name：{name}");
      }
    }

    /// <summary>
    /// 设置全局禁止交互的遮罩
    /// 同步用户数据时使用
    /// </summary>
    /// <param name="active"></param>
    public static void SetInteractionMask(bool active) {
      if (s_interactionMask) {
        s_interactionMask.SetActiveEx(active);
      } else {
        Debug.LogError("交互遮罩未正确设置！");
      }
    }

    /// <summary>
    /// 回退操作只适用于NORMAL层的UI
    /// </summary>
    public static void Back() {
      onBackStack.Peek()?.Invoke();
    }

    private static void _CloseInternal(UITemplate data, Stack<UIBase> stack, Action callback = null, params object[] args) {
      //NOTICE
      //Action的执行顺序要特别当心
      //先执行下面的再执行其他的回调
      //因为其他的回调里可能会执行打开UI的操作
      Action action = () => {
        //回收
        s_cacheUIListDict[data.uiType].Add((Time.realtimeSinceStartup, stack.Pop().Recycle()));
      };
      callback = action + callback;

      stack.Peek().Close(callback, args);
    }

    private static Transform _GetRootByType(UIType uiType) {
      switch (uiType) {
        case UIType.NORMAL: {
            return normalCanvas;
          }
        case UIType.TOP: {
            return topCanvas;
          }
        case UIType.TIP: {
            return tipCanvas;
          }
        default: {
            return null;
          }
      }
    }

    public static void Block(UIBase ui) {
      s_blockDict[ui.uiType]++;
    }

    public static void Unblock(UIBase ui) {
      s_blockDict[ui.uiType]--;
    }
  }
}