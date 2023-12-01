namespace Yume.AVG {
  public abstract class LeafNodeBase : AVGNodeBase {
    [Input(ShowBackingValue.Never, ConnectionType.Override)]
    public int node;
  }
}