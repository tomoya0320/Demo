using UnityEngine;

namespace Yume.AVG {
	public class PlaySound : LeafNodeBase {
    [SerializeField]
		private AudioClip _soundClip;

		public override void Run() {
			AudioManager.PlaySound(_soundClip);
		}
	}
}