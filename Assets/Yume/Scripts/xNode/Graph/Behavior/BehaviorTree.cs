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
    // 同一个buff下的所有行为树共用的Blackboard
    [NonSerialized]
    public Blackboard blackboard;
    
    public bool Run() {
      var node = nodes.Find(n => n is Root);
      return (node as BehaviorNodeBase)?.Run() ?? false;
    }
  }
}