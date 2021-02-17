using System;
using UnityEngine;
using XNode;

namespace Yume.Battle {
  [CreateAssetMenu]
  public class BehaviorTree : NodeGraph {
    [NonSerialized]
    public Unit unit;
    [NonSerialized]
    public Buff buff;
    // 这棵树自己的Blackboard 比如buff里的Blackboard
    [NonSerialized]
    public Blackboard blackboard;
    
    public bool Run() {
      var node = nodes.Find(n => n is Root);
      return (node as BehaviorNodeBase)?.Run() ?? false;
    }
  }
}