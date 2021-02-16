using System.Collections.Generic;

namespace Yume.Battle {
  public class AttachAttrib : LeafNodeBase {
    public override bool Run() {
      Unit owner = behaviorTree.unit;
      var attribAdditives = behaviorTree.blackboard.GetValue<AttribAdditives>("attribAdditives");
      if (attribAdditives == null) {
        return false;
      }


      var attribDetachments = behaviorTree.blackboard.GetValue<List<AttribArgs>>("attribDetachments");
      if (attribDetachments == null) {
        attribDetachments = new List<AttribArgs>();
        behaviorTree.blackboard["attribDetachments"] = attribDetachments;
      }
      foreach (var attribAdditive in attribAdditives) {
        float finalValue = attribAdditive.value;
        finalValue += attribAdditive.rateOnValue * owner.GetAttrib(attribAdditive.type);
        finalValue += attribAdditive.rateOnMaxValue * owner.GetAttrib(attribAdditive.type, true);
        float realValue = owner.AddAttrib(attribAdditive.type, finalValue, attribAdditive.onMaxValue);
        
        AttribArgs args = new AttribArgs {
          type = attribAdditive.type,
          onMaxValue = attribAdditive.onMaxValue,
          value = realValue
        };
        attribDetachments.Add(args);
      }

      return true;
    }
  }
}