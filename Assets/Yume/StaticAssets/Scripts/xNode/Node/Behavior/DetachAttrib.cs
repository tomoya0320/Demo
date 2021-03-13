using System.Collections.Generic;

namespace Yume.Battle {
  /// <summary>
  /// 通常与AttachAttrib节点配合使用
  /// </summary>
  public class DetachAttrib : LeafNodeBase {
    public override bool Run() {
      Unit owner = behaviorTree.unit;
      var attribDetachments = behaviorTree.blackboard.GetValueAndRemove<List<AttribArgs>>("attribDetachments");
      if (attribDetachments == null) {
        return false;
      }

      foreach (var attribDetachment in attribDetachments) {
        owner.AddAttrib(attribDetachment.type, -attribDetachment.value, attribDetachment.onMaxValue);
      }

      return true;
    }
  }
}