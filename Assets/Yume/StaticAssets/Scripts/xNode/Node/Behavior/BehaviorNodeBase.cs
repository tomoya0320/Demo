using XNode;

namespace Yume.Battle {
  public abstract class BehaviorNodeBase : Node {
    protected BehaviorTree behaviorTree;
    protected Unit owner => behaviorTree.unit;

    protected override void Init() {
      base.Init();
      behaviorTree = graph as BehaviorTree;
    }

    public abstract bool Run();
  }
}