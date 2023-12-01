namespace Yume.Battle {
  public abstract class ControllerNodeBase : BehaviorNodeBase {
    [Input(ShowBackingValue.Never, ConnectionType.Override)]
    public int last;
    [Output(ShowBackingValue.Never)]
    public int next;
  }
}