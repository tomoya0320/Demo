using UnityEngine;
using Yume.Audio;
using Yume.UI;

namespace Yume.AVG {
  public class PlayBgm : AVGNodeBase {
		[SerializeField]
		private AudioClip _bgmClip;

		public override void DoOperation(UIAVG avg) {
			AudioManager.PlayBgm(_bgmClip);
		}
	}
}