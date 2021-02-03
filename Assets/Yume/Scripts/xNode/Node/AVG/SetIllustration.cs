using UnityEngine;
using Yume.UI;

namespace Yume.AVG {
	public class SetIllustration : AVGNodeBase {
		[Input(ShowBackingValue.Never)]
		public int node;
		[SerializeField]
		private Sprite _illustrationSprite;

		public override void Run(UIAVG avg) {
			avg.illustration.sprite = _illustrationSprite;
		}
	}
}