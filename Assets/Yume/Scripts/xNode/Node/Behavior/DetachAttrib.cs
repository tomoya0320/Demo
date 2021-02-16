using System.Collections.Generic;

namespace Yume.Battle {
  public class DetachAttrib : LeafNodeBase {
    public override bool Run() {
      Unit owner = behaviorTree.unit;
      var attribDetachments = behaviorTree.blackboard.GetValue<List<AttribArgs>>("attribDetachments");
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