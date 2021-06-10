namespace Yume.Battle {
  public abstract class LeafNodeBase : BehaviorNodeBase {
    [Input(ShowBackingValue.Never, ConnectionType.Override)]
    public int last;
  }
}