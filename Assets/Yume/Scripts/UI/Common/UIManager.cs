using System;
using System.Collections.Generic;
using UnityEngine;
using Yume.Main;
using Yume.Resource;

namespace Yume.UI {
  public enum UIType {
    NORMAL, TOP, TIP
  }

  [Serializable]
  public struct UIData {
    public string name;
    public UIType uiType;
    public string typeName;
    public string prefabUrl;
    public float cacheTime;
    public bool isCloseLast;
  }

  public static class UIManager {
    private static GameObject s_interactionMask;
    public static Transform topCanvas { private set; get; }
    public static Transform tipCanvas { private set; get; }
    public static Transform cacheCanvas { private set; get; }
    public static Transform normalCanvas { private set; get; }
    private static Dictionary<string, UIData> s_uiDataDict = new Dictionary<string, UIData>();
    private static Dictionary<object, object> s_sharedArgsDict = new Dictionary<object, object>();
    private static Dictionary<UIType, Stack<UIBase>> s_uiStackDict = new Dictionary<UIType, Stack<UIBase>>();
    private static Dictionary<UIType, List<(float, UIBase)>> s_cacheUIListDict = new Dictionary<UIType, List<(float, UIBase)>>();

    private static Action s_quitGame = Application.Quit;

    public static bool activeBack { set; get; }

    public static void Init() {
      ObjectContainer uiRootObject = Game.instance.uiRootObject;
      topCanvas = uiRootObject.GetGameObject("TopCanvas")?.transform;
      tipCanvas = uiRootObject.GetGameObject("TipCanvas")?.transform;
      cacheCanvas = uiRootObject.GetGameObject("CacheCanvas")?.transform;
      normalCanvas = uiRootObject.GetGameObject("NormalCanvas")?.transform;
      s_interactionMask = uiRootObject.GetGameObject("InteractionMask");

      foreach (UIType type in Enum.GetValues(typeof(UIType))) {
        s_uiStackDict.Add(type, new Stack<UIBase>());
        s_cacheUIListDict.Add(type, new List<(float, UIBase)>());
      }

      Register(ResourceManager.LoadAll<UIData>());
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

    public static void Register(params UIData[] uiList) {
      foreach (var ui in uiList) {
        if (s_uiDataDict.ContainsKey(ui.name)) {
          Debug.LogWarning($"UI已注册！name：{ui.name}");
        }
        s_uiDataDict.Add(ui.name, ui);
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

    /// <summary>
    /// 打开一个UI并入栈同时选择性关闭上一个UI
    /// </summary>
    /// <param name="name"></param>
    /// <param name="args"></param>
    public static void OpenOne(string name, Action callback = null, params object[] args) {
      if (s_uiDataDict.ContainsKey(name)) {
        var data = s_uiDataDict[name];
        var stack = s_uiStackDict[data.uiType];

        if (stack.Count > 0 && data.isCloseLast) {
          stack.Peek().Close(() => {
            _OpenInternal(data, stack, callback, args);
          });
        } else {
          _OpenInternal(data, stack, callback, args);
        }
      } else {
        Debug.LogError($"UI未注册！name：{name}");
      }
    }

    private static void _OpenInternal(UIData data, Stack<UIBase> stack, Action callback = null, params object[] args) {
      UIBase ui = null;
      if (!_TryGetCache(data, ref ui)) {
        ui = AssemblyTool.CreateInstance(data.typeName) as UIBase;
        GameObject prefab = ResourceManager.Load<GameObject>(data.prefabUrl);
        if (prefab == null) {
          Debug.LogError($"Prefab为空！url:{data.prefabUrl}");
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
    /// <param name="uiData"></param>
    /// <returns></returns>
    private static bool _TryGetCache(UIData uiData, ref UIBase ui) {
      var index = s_cacheUIListDict[uiData.uiType].FindIndex(i => i.Item2.name == uiData.name);
      if (index >= 0) {
        ui = s_cacheUIListDict[uiData.uiType][index].Item2;
        s_cacheUIListDict[uiData.uiType].RemoveAt(index);
        ui.root.transform.SetParent(_GetRootByType(uiData.uiType));
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
      if (s_uiDataDict.ContainsKey(name)) {
        var data = s_uiDataDict[name];
        var stack = s_uiStackDict[data.uiType];

        while (stack.Count > 0) {
          stack.Pop().Destroy();
        }
        OpenOne(name, callback, args);
      } else {
        Debug.LogError($"UI未注册！name：{name}");
      }
    }

    /// <summary>
    /// 关闭栈顶UI并选择性打开上一个UI
    /// </summary>
    /// <param name="name"></param>
    /// <param name="args"></param>
    public static void CloseOne(string name, Action callback = null, params object[] args) {
      if (s_uiDataDict.ContainsKey(name)) {
        var data = s_uiDataDict[name];
        var stack = s_uiStackDict[s_uiDataDict[name].uiType];

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
        Debug.LogError($"UI未注册！name：{name}");
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
      if (activeBack) {
        if (isShowing("confirmation")) {
          CloseOne("confirmation");
        } else {
          if (s_uiStackDict[UIType.NORMAL].Count > 1) {
            CloseOne(s_uiStackDict[UIType.NORMAL].Peek().name);
          } else {
            OpenOne("confirmation", null, "确认退出游戏？", "确认", "取消", s_quitGame, null);
          }
        }
      }
    }

    /// <summary>
    /// 判断一个UI是否正在显示
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static bool isShowing(string name) {
      foreach (var stack in s_uiStackDict.Values) {
        foreach (var ui in stack) {
          if (ui.name == name) {
            return ui.isShowing;
          }
        }
      }

      return false;
    }

    private static void _CloseInternal(UIData data, Stack<UIBase> stack, Action callback = null, params object[] args) {
      //NOTICE
      //Action的执行顺序要特别当心
      //先执行下面的再执行其他的回调
      //因为其他的回调里可能会执行打开UI的操作
      Action action = () => {
        //回收
        s_cacheUIListDict[data.uiType].Add((Time.realtimeSinceStartup, stack.Pop().Recycle()));
        //选择性打开上一个UI
        if (stack.Count > 0 && data.isCloseLast) {
          stack.Peek().Open();
        }
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
  }
}