namespace Yume.Battle {
  public class BattleController : SingletonMono<BattleController> {
    protected override void OnBeforeInit() {
      dontDestroyOnLoad = false;
    }
  }
}