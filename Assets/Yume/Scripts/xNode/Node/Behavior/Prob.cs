namespace Yume.Battle {
  public class Prob : LeafNodeBase {
    public override bool Run() {
      float prob = behaviorTree.blackboard.GetValue<float>("prob");
      return MathUtil.Prob(prob);
    }
  }
}