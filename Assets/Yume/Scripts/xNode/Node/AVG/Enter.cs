using Yume.UI;

namespace Yume.AVG {
  public class Enter : AVGNodeBase {
    [Output(ShowBackingValue.Never)]
    public int next;

    public override void Run(UIAVG avg) {
      var connection = GetOutputPort(nameof(next)).Connection;
      avgGraph.avgNode = connection.node as AVGMainNode;
      avgGraph.Run(avg);
    }
  }
}