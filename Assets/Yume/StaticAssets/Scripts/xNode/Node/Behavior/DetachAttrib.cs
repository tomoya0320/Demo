namespace Yume.Battle {
  /// <summary>
  /// 通常与AttachAttrib节点配合使用
  /// </summary>
  public class DetachAttrib : LeafNodeBase {
    public override bool Run() {
      var realAttribAdditives = behaviorTree.blackboard.GetValueAndRemove<AttribAdditives>(Blackboard.REAL_ATTRIB_ADDITIVES);
      if (realAttribAdditives == null) {
        return false;
      }

      foreach (var realAttribAdditive in realAttribAdditives) {
        owner.AddAttrib(realAttribAdditive.type, -realAttribAdditive.value, realAttribAdditive.onMaxValue);
      }

      return true;
    }
  }
}