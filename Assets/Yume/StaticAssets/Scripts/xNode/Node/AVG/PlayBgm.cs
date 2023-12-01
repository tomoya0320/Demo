using UnityEngine;

namespace Yume.AVG {
  public class PlayBgm : LeafNodeBase {
    [SerializeField]
		private string _bgmName;

		public override void Run() {
			AudioManager.PlayBgm(_bgmName);
      avg.bgm = _bgmName;
    }
	}
}