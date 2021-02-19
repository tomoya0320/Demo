using UnityEditor;
using UnityEngine;
using Yume.Data;
using Yume.RedPointSystem;

namespace Yume.Editor {
  public class RedPointTreeEditorWin : EditorWindow {
    private bool m_showMailRedPointTree;
    private bool m_showTaskRedPointTree;

    [MenuItem("Tools/红点树逻辑查看")]
    public static void ShowWin() {
      GetWindow<RedPointTreeEditorWin>("红点树逻辑").Show();
    }

    private void OnGUI() {
      if (!Application.isPlaying) {
        EditorGUILayout.LabelField("游戏未启动");
        return;
      }

      if (!DataManager.isLogged) {
        EditorGUILayout.LabelField("玩家未登录");
        return;
      }

      m_showMailRedPointTree = EditorGUILayout.Foldout(m_showMailRedPointTree, "邮件红点树逻辑");
      if (m_showMailRedPointTree) {
        _DrawTree(RedPointTreeType.MAIN_MAIL);
      }

      m_showTaskRedPointTree = EditorGUILayout.Foldout(m_showTaskRedPointTree, "任务红点树逻辑");
      if (m_showTaskRedPointTree) {
        _DrawTree(RedPointTreeType.MAIN_TASK);
      }
    }

    private void _DrawTree(RedPointTreeType redPointTreeType) {
      var tree = RedPointManager.GetRedPointTree(redPointTreeType);
      foreach (var node in tree.redPointDict.Values) {
        EditorGUILayout.LabelField($"name:{node.name},parent:{node.parentName},redPointNum:{node.redPointNum}");
      }
    }
  }
}