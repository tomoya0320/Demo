using XNode;

namespace Yume.AVG {
  public class EnterNode : Node {
		[Output(ShowBackingValue.Never)]
		public int first;

    public override object GetValue(NodePort port) {
			// ‘› ±√ª”√
      return null;
    }

    public AVGNodeBase GetFirstAVGNode() {
			var connection = GetOutputPort(nameof(first)).Connection;
			if (connection != null) {
				return connection.node as AVGNodeBase;
			}
			return null;
		}
	}
}