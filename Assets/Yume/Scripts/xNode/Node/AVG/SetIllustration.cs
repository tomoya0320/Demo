using UnityEngine;
using Yume.UI;

namespace Yume.AVG {
	public class SetIllustration : LeafNodeBase {
    [SerializeField]
		private Sprite _illustrationSprite;

		public override void Run(UIAVG avg) {
			avg.illustration.sprite = _illustrationSprite;
		}
	}
}