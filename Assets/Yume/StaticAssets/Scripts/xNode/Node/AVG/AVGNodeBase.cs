using XNode;
using Yume.UI;

namespace Yume.AVG {
  public abstract class AVGNodeBase : Node {
    protected AVGGraph avgGraph;
    protected UIAVG avg => avgGraph.avg;
    
		protected override void Init() {
			base.Init();

			avgGraph = graph as AVGGraph;
    }

		public abstract void Run();
	}
}