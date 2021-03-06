using UnityEngine;
using Yume.Audio;
using Yume.UI;

namespace Yume.AVG {
	public class PlaySound : LeafNodeBase {
    [SerializeField]
		private AudioClip _soundClip;

		public override void Run(UIAVG avg) {
			AudioManager.PlaySound(_soundClip);
		}
	}
}