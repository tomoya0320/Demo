using UnityEngine;

namespace Yume.Battle {
  public class Parallel : ControllerNodeBase {
    public override bool Run() {
      bool result = false;
      var connections = GetOutputPort(nameof(next)).GetConnections();
      foreach (var connection in connections) {
        BehaviorNodeBase behaviorNode = connection.node as BehaviorNodeBase;
        if (behaviorNode == null) {
          Debug.LogError($"节点基类不匹配！类型:{connection.node.GetType().Name}");
          continue;
        }
        result |= behaviorNode.Run();
      }
      return result;
    }
  }
}