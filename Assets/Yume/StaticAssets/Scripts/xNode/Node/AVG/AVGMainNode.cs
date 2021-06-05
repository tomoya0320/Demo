namespace Yume.AVG {
  public class AVGMainNode : AVGNodeBase {
    [Input(ShowBackingValue.Never)]
    public int last;
    [Output(ShowBackingValue.Never, ConnectionType.Override)]
    public int next;
    [Output(ShowBackingValue.Never)]
    public int nodes;

    public override void Run() {
      var connections = GetOutputPort(nameof(nodes)).GetConnections();
      foreach (var connection in connections) {
        (connection.node as AVGNodeBase).Run();
      }
      avgGraph.avgNode = GetOutputPort(nameof(next)).Connection?.node as AVGNodeBase;
    }
  }
}