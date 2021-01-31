using Yume.UI;

namespace Yume.AVG {
  public class ParallelNode : AVGNodeBase {
		[Output]
		public AVGNodeBase avgNodes;

		public override void DoOperation(UIAVG avg) {
			var connections = GetOutputPort(nameof(avgNodes)).GetConnections();
			foreach (var connection in connections) {
				(connection.node as AVGNodeBase)?.DoOperation(avg);
			}
		}
	}
}