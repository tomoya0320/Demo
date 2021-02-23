using UnityEngine;
using Yume.Audio;
using Yume.UI;

namespace Yume.AVG {
  public class PlayBgm : LeafNode {
    [SerializeField]
		private AudioClip _bgmClip;

		public override void Run(UIAVG avg) {
			AudioManager.PlayBgm(_bgmClip);
		}
	}
}