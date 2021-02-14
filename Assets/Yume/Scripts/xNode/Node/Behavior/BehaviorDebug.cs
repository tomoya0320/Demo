using UnityEngine;

namespace Yume.Battle {
  public class BehaviorDebug : BehaviorNodeBase {
    [Input(ShowBackingValue.Never, ConnectionType.Override)]
    public int last;
    public string debugKey;
    
    public override bool Run() {
      Debug.Log(behaviorTree.blackboard.GetValue<object>(debugKey));
      return true;
    }
  }
}
