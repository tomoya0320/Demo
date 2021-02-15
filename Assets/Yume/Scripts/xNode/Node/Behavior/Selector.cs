using UnityEngine;

namespace Yume.Battle {
  public class Selector : BehaviorNodeBase {
    [Input(ShowBackingValue.Never)]
    public int last;
    [Output(ShowBackingValue.Never)]
    public int next;
    
    public override bool Run() {
      var connections = GetOutputPort(nameof(next)).GetConnections();
      foreach (var connection in connections) {
        BehaviorNodeBase behaviorNode = connection.node as BehaviorNodeBase;
        if (behaviorNode == null) {
          Debug.LogError($"节点基类不匹配！类型:{connection.node.GetType().Name}");
          continue;
        }
        if (behaviorNode.Run()) {
          return true;
        }
      }
      return false;
    }
  }
}