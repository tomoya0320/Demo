using Yume.UI;

namespace Yume.AVG {
	public class AVGMainNode : AVGNodeBase {
		[Input(ShowBackingValue.Never)]
		public int last;
		[Output(ShowBackingValue.Never)]
		public int next;
		[Output(ShowBackingValue.Never)]
		public int nodes;

		public override void Run(UIAVG avg) {
			var connections = GetOutputPort(nameof(nodes)).GetConnections();
			foreach (var connection in connections) {
				(connection.node as AVGNodeBase).Run(avg);
			}
			avgGraph.avgNode = GetOutputPort(nameof(next)).Connection.node as AVGNodeBase;
		}
	}
}