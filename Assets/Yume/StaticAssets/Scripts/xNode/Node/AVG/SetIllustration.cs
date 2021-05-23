using UnityEngine;

namespace Yume.AVG {
	public class SetIllustration : LeafNodeBase {
    [SerializeField]
		private Sprite _illustrationSprite;

		public override void Run() {
			avg.illustration.sprite = _illustrationSprite;
		}
	}
}