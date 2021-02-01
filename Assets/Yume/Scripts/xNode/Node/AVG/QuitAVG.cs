using Yume.UI;

namespace Yume.AVG {
  public class QuitAVG : AVGNodeBase {
    public override void DoOperation(UIAVG avg) {
      avgGraph.CleanUp();
      avg.QuitAVG();
    }
  }
}