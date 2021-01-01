using UnityEngine;
using UnityEngine.UI;
using Yume.RedPointSystem;
using Yume.String;

namespace Yume.UI {
  public class UIRedPoint {
    private string m_name;
    private bool m_showNum;
    private string m_parent;
    private int m_redPointNum;
    private Text m_redPointText;
    public string name => m_name;
    private GameObject m_redPointGo;
    public string parent => m_parent;
    private RedPointTreeType m_redPointTreeType;
    public RedPointTreeType redPointTreeType => m_redPointTreeType;

    public UIRedPoint(RedPointTreeType redPointTreeType, string name, string parent, GameObject redPointGo, bool showNum = false) {
      m_name = name;
      m_parent = parent;
      m_showNum = showNum;
      m_redPointGo = redPointGo;
      m_redPointTreeType = redPointTreeType;
      m_redPointText = m_redPointGo.GetComponentInChildren<Text>(true);

      RedPointManager.instance.AddUIRedPoint(this);
    }

    /// <summary>
    /// 配合ScrollView使用
    /// ScrollView里的每个节点的父节点需一致
    /// </summary>
    /// <param name="name"></param>
    public void Update(string name) {
      m_name = name;
      OnChanged();
    }

    public void OnChanged() {
      m_redPointNum = RedPointManager.instance.GetRedPointNum(m_redPointTreeType, m_name, m_parent);
      if (m_redPointNum > 0) {
        m_redPointGo.SetActiveEx(true);
        m_redPointText.text = m_showNum ? m_redPointNum.ToString() : StringMap.EMPTY;
      } else {
        m_redPointGo.SetActiveEx(false);
      }
    }

    public void Destroy() {
      RedPointManager.instance.RemoveUIRedPoint(this);
    }
  }
}