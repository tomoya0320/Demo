using UnityEngine;
using UnityEngine.UI;
using Yume.RedPointSystem;

namespace Yume.UI {
  public class UIRedPoint : UIChildBase {
    private string m_name;
    private bool m_showNum;
    private string m_parent;
    private int m_redPointNum;
    public string name => m_name;
    private Text m_redPointNumText;
    public string parent => m_parent;
    private RedPointTreeType m_redPointTreeType;
    public RedPointTreeType redPointTreeType => m_redPointTreeType;

    public UIRedPoint(RedPointTreeType redPointTreeType, string name, string parent, GameObject root, bool showNum = false) : base(root) {
      m_name = name;
      m_parent = parent;
      m_showNum = showNum;
      m_redPointTreeType = redPointTreeType;
      m_redPointNumText = GetComponent<Text>("RedPointNumText");

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
        gameObject.SetActiveEx(true);
        m_redPointNumText.text = m_showNum ? m_redPointNum.ToString() : StringMap.EMPTY;
      } else {
        gameObject.SetActiveEx(false);
      }
    }

    public override void Destroy() {
      RedPointManager.instance.RemoveUIRedPoint(this);
    }
  }
}