using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace Yume.RedPointSystem {
  /// <summary>
  /// Description为Tree的Type
  /// 一种红点树类型代表了一个红点树的子类
  /// </summary>
  public enum RedPointTreeType {
    NONE,//未实现的类型
    [Description("Yume.RedPointSystem.MainTaskRedPointTree")]
    MAIN_TASK,//主界面的任务
    [Description("Yume.RedPointSystem.MainMailRedPointTree")]
    MAIN_MAIL,//主界面的邮件
    [Description("Yume.RedPointSystem.MainItemRedPointTree")]
    MAIN_ITEM,//主界面的道具
  }

  public class RedPointNode {
    private string m_name;
    /// <summary>
    /// 如果要把RedPointNode移除的话(仅限移除叶节点)记得将RedPointNode父节点的这个变量减一
    /// </summary>
    private int m_childCount;
    private int m_redPointNum;
    public string name => m_name;
    private RedPointNode m_parentNode;
    public bool isLeaf => m_childCount == 0;
    public int redPointNum => m_redPointNum;
    public string parentName => m_parentNode?.name;

    public RedPointNode(string name, RedPointNode parentNode) {
      m_name = name;
      m_parentNode = parentNode;
      if (m_parentNode != null) {
        m_parentNode.m_childCount++;
      }
    }

    public void SetRedPointNum(int num) {
      if (num < 0) {
        Debug.LogError("RedPointNum不能小于0！");
      } else {
        int delta = num - m_redPointNum;
        if (delta != 0) {
          m_redPointNum = num;
          //更新父节点
          if (m_parentNode != null) {
            m_parentNode.SetRedPointNum(m_parentNode.m_redPointNum + delta);
          }
        }
      }
    }

    public void OnRemove() {
      m_parentNode.m_childCount--;
    }
  }

  public abstract class RedPointTree {
    protected RedPointTreeType redPointTreeType = RedPointTreeType.NONE;
    protected Dictionary<string, RedPointNode> redPointDict = new Dictionary<string, RedPointNode>();

    /// <summary>
    /// 每次被实例化时初始化具体的红点业务逻辑
    /// 由子类负责这部分
    /// </summary>
    public abstract RedPointTree OnInit();

    /// <summary>
    /// 当不存在该节点对时返回0
    /// </summary>
    /// <param name="name"></param>
    /// <param name="parent"></param>
    /// <returns></returns>
    public int GetRedPointNum(string name, string parent) {
      if(redPointDict.ContainsKey(name) && redPointDict[name].parentName == parent) {
        return redPointDict[name].redPointNum;
      }
      Debug.Log($"父子节点不存在 name:{name},parent:{parent}");
      return 0;
    }

    protected void InitLeafNode(string name, string parent, int initRedPointNum = 1) {
      if(string.IsNullOrEmpty(name) || string.IsNullOrEmpty(parent)) {
        Debug.LogError("父子节点名均不能为空！");
        return;
      }

      if (redPointDict.ContainsKey(name)) {
        Debug.LogError($"该子节点已添加！name:{name}");
      } else {
        if (redPointDict.ContainsKey(parent)) {
          redPointDict.Add(name, new RedPointNode(name, redPointDict[parent]));
          redPointDict[name].SetRedPointNum(initRedPointNum);
        } else {
          Debug.LogError($"找不到父节点！parent:{parent}");
        }
      }
    }

    public void SetRedPointNum(string name, string parent, int redPointNum) {
      if (redPointDict.ContainsKey(name)) {
        var redPointNode = redPointDict[name];
        if (redPointNode.parentName == parent) {
          if (redPointNode.isLeaf) {
            redPointNode.SetRedPointNum(redPointNum);
            if (redPointNode.redPointNum <= 0) {
              redPointDict.Remove(name);
              redPointNode.OnRemove();
            }
          } else {
            Debug.LogError($"外部不允许直接对非叶节点进行设置！name:{name},parent:{parent}");
          }
        } else {
          Debug.LogError($"父节点不一致！name:{name},parent:{parent},correctParent:{redPointNode.parentName}");
        }
      } else {
        InitLeafNode(name, parent, redPointNum);
      }
    }

#if UNITY_EDITOR
    public Dictionary<string, RedPointNode> RedPointDict => redPointDict;
#endif
  }

  /// <summary>
  /// 外部都通过这个类进行操作
  /// </summary>
  public class RedPointManager : Singleton<RedPointManager> {
    private Dictionary<RedPointTreeType, RedPointTree> m_redPointTreeDict = new Dictionary<RedPointTreeType, RedPointTree>();

    protected override void OnBeforeInit() {
      foreach (RedPointTreeType type in Enum.GetValues(typeof(RedPointTreeType))) {
        if (type != RedPointTreeType.NONE) {
          var redPointTree = AssemblyTool.CreateInstance(type.GetDescription()) as RedPointTree;
          m_redPointTreeDict.Add(type, redPointTree.OnInit());
        }
      }
    }

    public void SetRedPointNum(RedPointTreeType type, string name, string parent, int redPointNum) {
      if (string.IsNullOrEmpty(name)) {
        return;
      }

      if (m_redPointTreeDict.ContainsKey(type)) {
        m_redPointTreeDict[type].SetRedPointNum(name, parent, redPointNum);
      } else {
        Debug.LogError($"该类型的Tree未注册！Type:{type}");
      }
    }

    public int GetRedPointNum(RedPointTreeType type, string name,string parent) {
      if (string.IsNullOrEmpty(name)) {
        return 0;
      }

      if (m_redPointTreeDict.ContainsKey(type)) {
        return m_redPointTreeDict[type].GetRedPointNum(name, parent);
      } else {
        Debug.LogError($"该类型的Tree未注册！Type:{type}");
        return 0;
      }
    }

#if UNITY_EDITOR
    public RedPointTree GetRedPointTree(RedPointTreeType type) {
      if (m_redPointTreeDict.ContainsKey(type)) {
        return m_redPointTreeDict[type];
      }
      return null;
    }
#endif
  }
}