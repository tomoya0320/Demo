namespace Yume.Battle {
  public class Reverse : DecoratorNodeBase {
    public override bool Run() {
      var connection = GetOutputPort(nameof(next)).Connection;
      return !((connection.node as BehaviorNodeBase)?.Run() ?? false);
    }
  }
}
