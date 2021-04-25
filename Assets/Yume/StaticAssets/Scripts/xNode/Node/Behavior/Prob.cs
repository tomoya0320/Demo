namespace Yume.Battle {
  public class Prob : LeafNodeBase {
    public override bool Run() {
      float prob = behaviorTree.blackboard.GetValue<float>(Blackboard.PROB);
      return MathUtil.Prob(prob);
    }
  }
}