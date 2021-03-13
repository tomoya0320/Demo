using UnityEngine;

namespace Yume.Battle {
  public class Sequence : IntermediateNodeBase {
    public override bool Run() {
      var connections = GetOutputPort(nameof(next)).GetConnections();
      foreach (var connection in connections) {
        BehaviorNodeBase behaviorNode = connection.node as BehaviorNodeBase;
        if (behaviorNode == null) {
          Debug.LogError($"节点基类不匹配！类型:{connection.node.GetType().Name}");
          return false;
        }
        if (!behaviorNode.Run()) {
          return false;
        }
      }
      return true;
    }
  }
}
