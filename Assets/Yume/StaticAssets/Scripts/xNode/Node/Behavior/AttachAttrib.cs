namespace Yume.Battle {
  public class AttachAttrib : LeafNodeBase {
    public override bool Run() {
      var attribAdditives = behaviorTree.blackboard.GetValue<AttribAdditives>(Blackboard.ATTRIB_ADDITIVES);
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
        if (Colors.TryGetAttribColor(attribAdditive.type, out var color)) {
          NumFactory.ShowNum(realValue, owner.numReference.position, color);
        }

        AttribAdditive args = new AttribAdditive {
          type = attribAdditive.type,
          onMaxValue = attribAdditive.onMaxValue,
          value = realValue
        };
        realAttribAdditives.Add(args);
      }
      behaviorTree.blackboard[Blackboard.REAL_ATTRIB_ADDITIVES] = realAttribAdditives;

      return true;
    }
  }
}