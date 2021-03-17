using UnityEngine;

namespace Yume.Battle {
  public class AttachAttrib : LeafNodeBase {
    public override bool Run() {
      Unit owner = behaviorTree.unit;
      var attribAdditives = behaviorTree.blackboard.GetValue<AttribAdditives>("attribAdditives");
      if (attribAdditives == null) {
        return false;
      }

      AttribAdditives realAttribAdditives = new AttribAdditives();
      foreach (var attribAdditive in attribAdditives) {
        int finalValue = attribAdditive.value;
        finalValue += attribAdditive.rateOnValue * owner.GetAttrib(attribAdditive.type) / Constant.THOUSAND;
        finalValue += attribAdditive.rateOnMaxValue * owner.GetAttrib(attribAdditive.type, true) / Constant.THOUSAND;
        int realValue = owner.AddAttrib(attribAdditive.type, finalValue, attribAdditive.onMaxValue);

        // 显示数字
        Color color = Colors.GetAttribColor(attribAdditive.type);
        NumFactory.ShowNum(realValue, owner.numReference.position, color);

        AttribAdditive args = new AttribAdditive {
          type = attribAdditive.type,
          onMaxValue = attribAdditive.onMaxValue,
          value = realValue
        };
        realAttribAdditives.Add(args);
      }
      behaviorTree.blackboard["realAttribAdditives"] = realAttribAdditives;

      return true;
    }
  }
}