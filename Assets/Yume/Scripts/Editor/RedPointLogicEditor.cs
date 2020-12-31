using UnityEditor;
using Yume.RedPointSystem;

namespace Yume.Editor {
  public class RedPointLogicEditor : EditorWindow {
    private bool m_showMailRedPointTree;
    private bool m_showTaskRedPointTree;
    private bool m_showItemRedPointTree;

    [MenuItem("Tools/红点树逻辑查看")]
    public static void ShowWin() {
      GetWindow<RedPointLogicEditor>().Show();
    }

    private void OnGUI() {
      m_showMailRedPointTree = EditorGUILayout.Foldout(m_showMailRedPointTree, "邮件红点树逻辑");  
      if (m_showMailRedPointTree) {
        var tree = RedPointManager.instance.GetRedPointTree(RedPointTreeType.MAIN_MAIL);
        foreach (var node in tree.RedPointDict.Values) {
          EditorGUILayout.LabelField($"\tname:{node.name},parent:{node.parentName},redPointNum:{node.redPointNum}");
        }
      }

      m_showTaskRedPointTree = EditorGUILayout.Foldout(m_showTaskRedPointTree, "任务红点树逻辑");
      if (m_showTaskRedPointTree) {
        var tree = RedPointManager.instance.GetRedPointTree(RedPointTreeType.MAIN_TASK);
        foreach (var node in tree.RedPointDict.Values) {
          EditorGUILayout.LabelField($"\tname:{node.name},parent:{node.parentName},redPointNum:{node.redPointNum}");
        }
      }

      m_showItemRedPointTree = EditorGUILayout.Foldout(m_showItemRedPointTree, "道具红点树逻辑");
      if (m_showItemRedPointTree) {
        var tree = RedPointManager.instance.GetRedPointTree(RedPointTreeType.MAIN_ITEM);
        foreach (var node in tree.RedPointDict.Values) {
          EditorGUILayout.LabelField($"\tname:{node.name},parent:{node.parentName},redPointNum:{node.redPointNum}");
        }
      }
    }
  }
}