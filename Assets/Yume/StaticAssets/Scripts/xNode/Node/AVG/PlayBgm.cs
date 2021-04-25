using UnityEngine;
using Yume.UI;

namespace Yume.AVG {
  public class PlayBgm : LeafNodeBase {
    [SerializeField]
		private AudioClip _bgmClip;

		public override void Run(UIAVG avg) {
			AudioManager.PlayBgm(_bgmClip);
      avg.CacheBgm();
    }
	}
}