using UnityEngine;
using UnityEngine.UI;
using Yume.RedDotSystem;

namespace Yume.UI {
  public class UIRedDot : UIChildBase {
    private string m_name;
    private bool m_showNum;
    private string m_parent;
    private int m_redDotNum;
    public string name => m_name;
    private Text m_redDotNumText;
    public string parent => m_parent;
    private RedDotTreeType m_redDotTreeType;
    public RedDotTreeType redDotTreeType => m_redDotTreeType;

    public UIRedDot(RedDotTreeType redDotTreeType, string name, string parent, GameObject root, bool showNum = false) : base(root) {
      m_name = name;
      m_parent = parent;
      m_showNum = showNum;
      m_redDotTreeType = redDotTreeType;
      m_redDotNumText = GetComponent<Text>("RedDotNumText");

      RedDotManager.AddUIRedDot(this);
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

    /// <summary>
    /// 红点UI监听
    /// </summary>
    public void OnChanged() {
      m_redDotNum = RedDotManager.GetRedDotNum(m_redDotTreeType, m_name, m_parent);
      if (m_redDotNum > 0) {
        gameObject.SetActiveEx(true);
        m_redDotNumText.text = m_showNum ? m_redDotNum.ToString() : StringMap.EMPTY;
      } else {
        gameObject.SetActiveEx(false);
      }
    }

    public override void Destroy() {
      RedDotManager.RemoveUIRedDot(this);
    }
  }
}