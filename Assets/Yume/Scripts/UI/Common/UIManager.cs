using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;
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
    public bool isAsyncLoad;
    public bool isCloseLast;
  }

  public class UIManager : Singleton<UIManager> {
    private Transform m_topCanvas;
    private Transform m_tipCanvas;
    private Transform m_normalCanvas;
    public Transform cacheCanvas { private set; get; }
    private Dictionary<string, UIData> m_uiDataDict = new Dictionary<string, UIData>();
    private Dictionary<object, object> m_sharedArgsDict = new Dictionary<object, object>();
    private Dictionary<UIType, Stack<UIBase>> m_uiStackDict = new Dictionary<UIType, Stack<UIBase>>();
    private Dictionary<UIType, List<(float, UIBase)>> m_cacheUIListDict = new Dictionary<UIType, List<(float, UIBase)>>();

    protected override void OnInit() {
      base.OnInit();
      m_topCanvas = GameObject.Find("TopCanvas")?.transform;
      m_tipCanvas = GameObject.Find("TipCanvas")?.transform;
      cacheCanvas = GameObject.Find("CacheCanvas")?.transform;
      m_normalCanvas = GameObject.Find("NormalCanvas")?.transform;

      foreach (UIType type in Enum.GetValues(typeof(UIType))) {
        m_uiStackDict.Add(type, new Stack<UIBase>());
        m_cacheUIListDict.Add(type, new List<(float, UIBase)>());
      }
    }

    public void UpdateCache() {
      foreach (var list in m_cacheUIListDict.Values) {
        for (int i = list.Count - 1; i >= 0; i--) {
          if (list[i].Item1 + list[i].Item2.cacheTime <= Time.realtimeSinceStartup) {
            list[i].Item2.Destroy();
            list.RemoveAt(i);
          }
        }
      }
    }

    public void RegisterByJson() {
      var uiDataDict = JsonConvert.DeserializeObject<Dictionary<string, UIData>>(AssetReader.instance.jsonAsset.ui.text);
      foreach (var uiData in uiDataDict.Values) {
        Register(uiData);
      }
    }

    public bool Register(UIData uIData) {
      if (m_uiDataDict.ContainsKey(uIData.name)) {
        Debug.LogWarning($"UI已注册！name：{uIData.name}");
        return false;
      }
      m_uiDataDict.Add(uIData.name, uIData);
      return true;
    }

    public void SetSharedArgs(object key, object value) {
      if (m_sharedArgsDict.ContainsKey(key)) {
        m_sharedArgsDict[key] = value;
      } else {
        m_sharedArgsDict.Add(key, value);
      }
    }

    public object GetSharedArgs(object key) {
      if (m_sharedArgsDict.ContainsKey(key)) {
        return m_sharedArgsDict[key];
      } else {
        return null;
      }
    }

    /// <summary>
    /// 打开一个UI并入栈同时选择性关闭上一个UI
    /// </summary>
    /// <param name="name"></param>
    /// <param name="args"></param>
    public async void OpenOne(string name, params object[] args) {
      if (m_uiDataDict.ContainsKey(name)) {
        UIBase ui = null;
        var data = m_uiDataDict[name];

        if (!_TryGetCache(data, ref ui)) {
          ui = AssemblyTool.CreateInstance(data.typeName) as UIBase;
          GameObject prefab;
          if (data.isAsyncLoad) {
            prefab = await ResourceManager.LoadAssetAsync<GameObject>(data.prefabUrl);
          } else {
            prefab = ResourceManager.LoadAsset<GameObject>(data.prefabUrl);
          }
          if (prefab == null) {
            Debug.LogError($"Prefab为空！url:{data.prefabUrl}");
            return;
          }
          ui.OnInit(data, UnityEngine.Object.Instantiate(prefab, _GetRootByType(data.uiType)));
        }

        var stack = m_uiStackDict[data.uiType];

        if (stack.Count > 0 && data.isCloseLast) {
          stack.Peek().Close();
        }

        ui.Open(args);
        stack.Push(ui);
      } else {
        Debug.LogError($"UI未注册！name：{name}");
      }
    }

    /// <summary>
    /// 尝试从缓存中获取UI
    /// </summary>
    /// <param name="uiData"></param>
    /// <returns></returns>
    private bool _TryGetCache(UIData uiData, ref UIBase ui) {
      var index = m_cacheUIListDict[uiData.uiType].FindIndex(i => i.Item2.name == uiData.name);
      if (index >= 0) {
        ui = m_cacheUIListDict[uiData.uiType][index].Item2;
        m_cacheUIListDict[uiData.uiType].RemoveAt(index);
        ui.root.transform.SetParent(_GetRootByType(uiData.uiType));
        return true;
      }
      return false;
    }

    /// <summary>
    /// 关闭栈中的所有UI并打开新的UI(关闭过程不会打开上一个UI以实现无缝衔接)
    /// 相当于场景跳转
    /// </summary>
    /// <param name="name"></param>
    /// <param name="args"></param>
    public void CloseAllAndOpenOne(string name, params object[] args) {
      if (m_uiDataDict.ContainsKey(name)) {
        var stack = m_uiStackDict[m_uiDataDict[name].uiType];
        while (stack.Count > 0) {
          _CloseInternal(stack.Pop());
        }
        OpenOne(name, args);
      } else {
        Debug.LogError($"UI未注册！name：{name}");
      }
    }

    /// <summary>
    /// 关闭栈顶UI并选择性打开上一个UI
    /// </summary>
    /// <param name="name"></param>
    /// <param name="args"></param>
    public void CloseOne(string name, params object[] args) {
      if (m_uiDataDict.ContainsKey(name)) {
        var stack = m_uiStackDict[m_uiDataDict[name].uiType];
        if (stack.Count == 0) {
          Debug.LogError("UI栈为空！");
          return;
        }
        if (stack.Peek().name == name) {
          _CloseInternal(stack.Pop(), args);
          if (stack.Count > 0 && m_uiDataDict[name].isCloseLast) {
            stack.Peek().Open();
          }
        } else {
          Debug.LogError($"UI未在栈顶！name：{name}");
        }
      } else {
        Debug.LogError($"UI未注册！name：{name}");
      }
    }

    /// <summary>
    /// 关闭一个UIBase实例并回收进缓存字典
    /// </summary>
    /// <param name="ui"></param>
    /// <param name="args"></param>
    private void _CloseInternal(UIBase ui, params object[] args) {
      ui.Close(args);
      ui.Recycle(args);
      m_cacheUIListDict[ui.uiType].Add((Time.realtimeSinceStartup, ui));
    }

    private Transform _GetRootByType(UIType uiType) {
      switch (uiType) {
        case UIType.NORMAL: {
            return m_normalCanvas;
          }
        case UIType.TOP: {
            return m_topCanvas;
          }
        case UIType.TIP: {
            return m_tipCanvas;
          }
        default: {
            return null;
          }
      }
    }
  }
}