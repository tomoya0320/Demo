namespace Yume.Battle {
  public class IsOwnerAlive : LeafNodeBase {
    public override bool Run() {
      return owner.isAlive;
    }
  }
}