namespace Yume.AVG {
  public class QuitAVG : AVGNodeBase {
    [Input(ShowBackingValue.Never)]
    public int last;

    public override void Run() {
      avg.QuitAVG();
    }
  }
}