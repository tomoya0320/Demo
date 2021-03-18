namespace Yume.Battle {
  /// <summary>
  /// 通常与AttachAttrib节点配合使用
  /// </summary>
  public class DetachAttrib : LeafNodeBase {
    public override bool Run() {
      var realAttribAdditives = behaviorTree.blackboard.GetValueAndRemove<AttribAdditives>("realAttribAdditives");
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