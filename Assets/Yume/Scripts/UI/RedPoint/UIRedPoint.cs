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
    private GameObject m_redPointGo;
    private RedPointTreeType m_type;

    public UIRedPoint(RedPointTreeType type, string name, string parent, GameObject redPointGo, bool showNum = false) {
      m_type = type;
      m_name = name;
      m_parent = parent;
      m_showNum = showNum;
      m_redPointGo = redPointGo;
      m_redPointText = m_redPointGo.GetComponentInChildren<Text>(true);
    }
  }
}