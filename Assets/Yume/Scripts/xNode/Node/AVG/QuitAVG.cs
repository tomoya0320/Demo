using Yume.UI;

namespace Yume.AVG {
  public class QuitAVG : AVGNodeBase {
    [Input(ShowBackingValue.Never)]
    public int last;

    public override void Run(UIAVG avg) {
      avg.QuitAVG();
    }
  }
}