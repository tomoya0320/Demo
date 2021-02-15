namespace Yume.Battle {
  public class Root : BehaviorNodeBase {
    [Output(ShowBackingValue.Never, ConnectionType.Override)]
    public int next;

    public override bool Run() {
      var connection = GetOutputPort(nameof(next)).Connection;
      return (connection.node as BehaviorNodeBase)?.Run() ?? false;
    }
  }
}