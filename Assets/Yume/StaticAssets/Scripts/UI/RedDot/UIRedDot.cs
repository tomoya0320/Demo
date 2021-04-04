using UnityEngine;
using UnityEngine.UI;

namespace Yume.UI {
  public class UIRedDot : UIChildBase {
    private bool m_showNum;
    private int m_redDotNum;
    private Text m_redDotNumText;
    public string name { private set; get; }
    public string parent { private set; get; }
    public RedDotTreeType redDotTreeType { get; }

    public UIRedDot(RedDotTreeType redDotTreeType, string name, string parent, Transform transform, bool showNum = false) : base(transform.gameObject) {
      this.name = name;
      this.parent = parent;
      this.redDotTreeType = redDotTreeType;
      m_showNum = showNum;
      m_redDotNumText = GetComponent<Text>("RedDotNumText");

      RedDotManager.AddUIRedDot(this);
    }

    /// <summary>
    /// 配合ScrollView使用
    /// ScrollView里的每个节点的父节点需一致
    /// </summary>
    /// <param name="name"></param>
    public void Update(string name) {
      this.name = name;
      OnChanged();
    }

    /// <summary>
    /// 红点UI监听
    /// </summary>
    public void OnChanged() {
      m_redDotNum = RedDotManager.GetRedDotNum(redDotTreeType, name, parent);
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