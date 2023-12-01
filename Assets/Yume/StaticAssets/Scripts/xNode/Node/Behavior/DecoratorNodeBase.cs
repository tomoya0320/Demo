namespace Yume.Battle {
  public abstract class DecoratorNodeBase : BehaviorNodeBase {
    [Input(ShowBackingValue.Never, ConnectionType.Override)]
    public int last;
    [Output(ShowBackingValue.Never, ConnectionType.Override)]
    public int next;
  }
}