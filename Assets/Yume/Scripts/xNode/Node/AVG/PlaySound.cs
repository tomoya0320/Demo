using UnityEngine;
using Yume.Audio;
using Yume.UI;

namespace Yume.AVG {
	public class PlaySound : AVGNodeBase {
		[SerializeField]
		private AudioClip _soundClip;

		public override void DoOperation(UIAVG avg) {
			AudioManager.PlaySound(_soundClip);
		}
	}
}