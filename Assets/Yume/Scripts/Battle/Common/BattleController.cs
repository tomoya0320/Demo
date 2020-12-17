namespace Yume.Battle {
  public class BattleController : SingletonMono<BattleController> {
    protected override void OnInit() {
      dontDestroyOnLoad = false;
      base.OnInit();
    }
  }
}