using UnityEditor;
using UnityEditor.UI;
using Yume.UI;

namespace Yume.Editor {
  //指定我们要自定义编辑器的脚本
  [CustomEditor(typeof(YButton), true)]
  //使用了 SerializedObject 和 SerializedProperty 系统，因此，可以自动处理“多对象编辑”，“撤销undo” 和 “预制覆盖prefab override”。
  [CanEditMultipleObjects]
  public class YButtonEditor : ButtonEditor {
    //对应我们在MyButton中创建的字段
    //PS:需要注意一点，使用SerializedProperty 必须在类的字段前加[SerializeField]
    private SerializedProperty m_textComponent;
    private SerializedProperty m_overrideSoundName;
    private SerializedProperty m_soundEvenDisabled;
    private SerializedProperty m_mute;

    protected override void OnEnable() {
      base.OnEnable();
      m_textComponent = serializedObject.FindProperty("_textComponent");
      m_overrideSoundName = serializedObject.FindProperty("_overrideSoundName");
      m_soundEvenDisabled = serializedObject.FindProperty("_soundEvenDisabled");
      m_mute = serializedObject.FindProperty("_mute");
    }
    //并且特别注意，如果用这种序列化方式，需要在 OnInspectorGUI 开头和结尾各加一句 serializedObject.Update();  serializedObject.ApplyModifiedProperties();
    public override void OnInspectorGUI() {
      base.OnInspectorGUI();
      serializedObject.Update();
      //显示我们创建的属性
      EditorGUILayout.PropertyField(m_textComponent);
      EditorGUILayout.PropertyField(m_overrideSoundName);
      EditorGUILayout.PropertyField(m_soundEvenDisabled);
      EditorGUILayout.PropertyField(m_mute);
      serializedObject.ApplyModifiedProperties();
    }
  }
}