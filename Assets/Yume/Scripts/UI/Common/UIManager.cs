using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Yume.DB;
using Yume.Resource;
using Yume.Singleton;

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
    public bool isAsyncLoad;
  }

  public class UIManager : SingletonNoMono<UIManager> {
    private GameObject m_topCanvas;
    private GameObject m_tipCanvas;
    private GameObject m_normalCanvas;
    private Dictionary<string, UIData> m_uiDataDict = new Dictionary<string, UIData>();
    private Dictionary<UIType, Stack<UIBase>> m_uiStackDict = new Dictionary<UIType, Stack<UIBase>>();

    protected override void OnInit() {
      base.OnInit();
      m_topCanvas = GameObject.Find("TopCanvas");
      m_tipCanvas = GameObject.Find("TipCanvas");
      m_normalCanvas = GameObject.Find("NormalCanvas");
    }

    public void RegisterUIByJson() {
      var uiDataDict = JsonConvert.DeserializeObject<Dictionary<string, UIData>>(AssetReader.instance.jsonAsset.ui.text);
      foreach (var uiData in uiDataDict.Values) {
        RegisterUI(uiData);
      }
    }

    public bool RegisterUI(UIData uIData) {
      if (m_uiDataDict.ContainsKey(uIData.name)) {
        Debug.LogWarning($"UI已注册！name：{uIData.name}");
        return false;
      }
      m_uiDataDict.Add(uIData.name, uIData);
      return true;
    }

    public async void OpenUI(string name, bool closeLast = true, params object[] paramArray) {
      if (m_uiDataDict.ContainsKey(name)) {
        var data = m_uiDataDict[name];
        UIBase ui = (UIBase)AssemblyTool.CreateInstance(data.typeName);
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

        ui.OnInit(data, _CreateUI(data.uiType, prefab));
        if (m_uiStackDict.ContainsKey(data.uiType)) {
          if (closeLast) {
            m_uiStackDict[data.uiType].Peek().Close(paramArray);
          }
          m_uiStackDict[data.uiType].Push(ui);
        } else {
          var stack = new Stack<UIBase>();
          stack.Push(ui);
          m_uiStackDict.Add(data.uiType, stack);
        }
        ui.Open(paramArray);
      } else {
        Debug.LogError($"UI未注册！name：{name}");
      }
    }

    public void Close(string name, bool openLast = true, params object[] paramArray) {
      Stack<UIBase> s = null;
      foreach (var stack in m_uiStackDict.Values) {
        foreach (var ui in stack) {
          if (ui.name == name) {
            s = stack;
            break;
          }
        }
      }
      if (s == null) {
        Debug.LogError($"UI打开！name：{name}");
        return;
      }
      while (true) {
        var ui = s.Pop();
        ui.Close(paramArray);
        if(ui.name == name) {
          break;
        }
      }
      if (s.Count > 0 && openLast) {
        s.Peek().Open(paramArray);
      }
    }

    private GameObject _CreateUI(UIType uiType, GameObject prefab) {
      var obj = UnityEngine.Object.Instantiate(prefab);
      var pos = prefab.GetRect().anchoredPosition;
      switch (uiType) {
        case UIType.NORMAL: {
            obj.GetRect().SetParent(m_normalCanvas.transform);
            break;
          }
        case UIType.TOP: {
            obj.GetRect().SetParent(m_topCanvas.transform);
            break;
          }
        case UIType.TIP: {
            obj.GetRect().SetParent(m_tipCanvas.transform);
            break;
          }
      }
      obj.GetRect().anchoredPosition = pos;
      return obj;
    }

    public void FadeBlackMask(float fadeTime = 0.3f, Action onFadeInComplete = null, Action onFadeOutComplete = null) {
      
    }

    public void OpenConfirmationBox(string tip, string okText, string cancelText, UnityAction onOk = null, UnityAction onCancel = null) {
      
    }
  }
}