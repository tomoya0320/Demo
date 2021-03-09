using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using Yume.UI;

namespace Yume.RedDotSystem {
  /// <summary>
  /// Description为Tree的Type
  /// 一种红点树类型代表了一个红点树的子类
  /// </summary>
  public enum RedDotTreeType {
    [Description("NotImplemented")]
    NONE,//未实现的类型
    [Description("Yume.RedDotSystem.MainTaskRedDotTree")]
    MAIN_TASK,//主界面的任务
    [Description("Yume.RedDotSystem.MainMailRedDotTree")]
    MAIN_MAIL,//主界面的邮件
  }

  public class RedDotNode {
    private string m_name;
    /// <summary>
    /// 如果要把RedDotNode移除的话(仅限移除叶节点)记得将RedDotNode父节点的这个变量减一
    /// </summary>
    private int m_childCount;
    private int m_redDotNum;
    public string name => m_name;
    private RedDotNode m_parentNode;
    public bool isLeaf => m_childCount == 0;
    public int redDotNum => m_redDotNum;
    private RedDotTreeType m_redDotTreeType;
    public string parentName => m_parentNode?.name;

    public RedDotNode(RedDotTreeType redDotTreeType, string name, RedDotNode parentNode) {
      m_name = name;
      m_parentNode = parentNode;
      m_redDotTreeType = redDotTreeType;

      if (m_parentNode != null) {
        m_parentNode.m_childCount++;
      }
    }

    public void SetRedDotNum(int num) {
      if (num < 0) {
        Debug.LogError("RedDotNum不能小于0！");
      } else {
        int delta = num - m_redDotNum;
        if (delta != 0) {
          m_redDotNum = num;
          RedDotManager.GetUIOnChanged(m_redDotTreeType, m_name, parentName)?.Invoke();
          //更新父节点
          m_parentNode?.SetRedDotNum(m_parentNode.m_redDotNum + delta);
        }
      }
    }

    public void OnRemove() {
      m_parentNode.m_childCount--;
    }
  }

  public abstract class RedDotTree {
    protected Dictionary<string, RedDotNode> redDotDict;
    protected abstract RedDotTreeType redDotTreeType { get; }

    /// <summary>
    /// 每次被实例化时初始化具体的红点业务逻辑
    /// </summary>
    public abstract void InitData();

    /// <summary>
    /// 当不存在该节点对时返回0
    /// </summary>
    /// <param name="name"></param>
    /// <param name="parent"></param>
    /// <returns></returns>
    public int GetRedDotNum(string name, string parent) {
      if (redDotDict.ContainsKey(name) && redDotDict[name].parentName == parent) {
        return redDotDict[name].redDotNum;
      }
      return 0;
    }

    protected void AddLeafNode(string name, string parent, int initRedDotNum = 1) {
      if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(parent)) {
        Debug.LogError("父子节点名均不能为空！");
        return;
      }

      if (redDotDict.ContainsKey(name)) {
        Debug.LogError($"该子节点已添加！name:{name}");
      } else {
        if (redDotDict.ContainsKey(parent)) {
          redDotDict.Add(name, new RedDotNode(redDotTreeType, name, redDotDict[parent]));
          redDotDict[name].SetRedDotNum(initRedDotNum);
        } else {
          Debug.LogError($"找不到父节点！parent:{parent}");
        }
      }
    }

    public void SetRedDotNum(string name, string parent, int redDotNum) {
      if (redDotDict.ContainsKey(name)) {
        var redDotNode = redDotDict[name];
        if (redDotNode.parentName == parent) {
          if (redDotNode.isLeaf) {
            redDotNode.SetRedDotNum(redDotNum);
            if (redDotNode.redDotNum <= 0) {
              redDotDict.Remove(name);
              redDotNode.OnRemove();
            }
          } else {
            Debug.LogError($"外部不允许直接对非叶节点进行设置！name:{name},parent:{parent}");
          }
        } else {
          Debug.LogError($"父节点不一致！name:{name},parent:{parent},correctParent:{redDotNode.parentName}");
        }
      } else {
        AddLeafNode(name, parent, redDotNum);
      }
    }
  }

  /// <summary>
  /// 外部都通过这个类进行操作
  /// </summary>
  public static class RedDotManager {
    private static List<UIRedDot> s_uiRedDots = new List<UIRedDot>();
    private static Dictionary<RedDotTreeType, RedDotTree> s_redDotTreeDict = new Dictionary<RedDotTreeType, RedDotTree>();

    public static void Init() {
      foreach (RedDotTreeType redDotTreeType in Enum.GetValues(typeof(RedDotTreeType))) {
        if (redDotTreeType != RedDotTreeType.NONE) {
          var redDotTree = AssemblyUtil.CreateInstance(redDotTreeType.GetDescription()) as RedDotTree;
          s_redDotTreeDict.Add(redDotTreeType, redDotTree);
        }
      }
    }

    public static void InitData() {
      foreach (var redDotTree in s_redDotTreeDict.Values) {
        redDotTree.InitData();
      }
    }

    public static void SetRedDotNum(RedDotTreeType redDotTreeType, string name, string parent, int redDotNum) {
      if (string.IsNullOrEmpty(name)) {
        return;
      }

      if (s_redDotTreeDict.ContainsKey(redDotTreeType)) {
        s_redDotTreeDict[redDotTreeType].SetRedDotNum(name, parent, redDotNum);
      } else {
        Debug.LogError($"该类型的Tree未注册！Type:{redDotTreeType}");
      }
    }

    public static int GetRedDotNum(RedDotTreeType redDotTreeType, string name, string parent) {
      if (string.IsNullOrEmpty(name)) {
        return 0;
      }

      if (s_redDotTreeDict.ContainsKey(redDotTreeType)) {
        return s_redDotTreeDict[redDotTreeType].GetRedDotNum(name, parent);
      } else {
        Debug.LogError($"该类型的Tree未注册！Type:{redDotTreeType}");
        return 0;
      }
    }

    public static void AddUIRedDot(UIRedDot uiRedDot) {
      if (!s_uiRedDots.Contains(uiRedDot)) {
        s_uiRedDots.Add(uiRedDot);
        //添加的时候就执行一次，减少冗余的代码
        uiRedDot.OnChanged();
      } else {
        Debug.LogWarning($"红点UI已添加 name:{uiRedDot.name},parent:{uiRedDot.parent}");
      }
    }

    public static void RemoveUIRedDot(UIRedDot uiRedDot) {
      if (s_uiRedDots.Contains(uiRedDot)) {
        s_uiRedDots.Remove(uiRedDot);
      } else {
        Debug.LogError($"红点UI未添加！name:{uiRedDot.name},parent:{uiRedDot.parent}");
      }
    }

    public static Action GetUIOnChanged(RedDotTreeType redDotTreeType, string name, string parent) {
      var uiRedDot = s_uiRedDots.Find(ui => ui.redDotTreeType == redDotTreeType &&
                                             ui.name == name && ui.parent == parent);
      if (uiRedDot == null) {
        return null;
      } else {
        return uiRedDot.OnChanged;
      }
    }
  }
}