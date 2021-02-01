using Yume.UI;

namespace Yume.AVG {
  public class QuitAVG : AVGNodeBase {
    [Input(ShowBackingValue.Never)]
    public int node;

    public override void Run(UIAVG avg) {
      avgGraph.CleanUp();
      avg.QuitAVG();
    }
  }
}