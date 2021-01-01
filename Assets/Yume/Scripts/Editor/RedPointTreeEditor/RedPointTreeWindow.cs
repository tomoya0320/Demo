using UnityEditor;
using UnityEngine;
using Yume.DB;
using Yume.RedPointSystem;

namespace Yume.Editor {
  public class RedPointTreeWindow : EditorWindow {
    private bool m_showMailRedPointTree;
    private bool m_showTaskRedPointTree;
    private bool m_showItemRedPointTree;

    [MenuItem("Tools/红点树逻辑查看")]
    public static void ShowWin() {
      var win = GetWindow<RedPointTreeWindow>();
      win.titleContent.text = "红点树系统";
      win.titleContent.image = Resources.Load<Texture>("red point");
      win.Show();
    }

    private void OnGUI() {
      if (!Application.isPlaying) {
        EditorGUILayout.LabelField("游戏未启动");
        return;
      }

      if (!DBManager.instance.isLogged) {
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

      m_showItemRedPointTree = EditorGUILayout.Foldout(m_showItemRedPointTree, "道具红点树逻辑");
      if (m_showItemRedPointTree) {
        _DrawTree(RedPointTreeType.MAIN_ITEM);
      }
    }

    private void _DrawTree(RedPointTreeType redPointTreeType) {
      var tree = RedPointManager.instance.GetRedPointTree(redPointTreeType);
      foreach (var node in tree.RedPointDict.Values) {
        EditorGUILayout.LabelField($"\tname:{node.name},parent:{node.parentName},redPointNum:{node.redPointNum}");
      }
    }
  }
}