using UnityEngine;
using Yume.UI;

namespace Yume.AVG {
	public class SetIllustration : AVGNodeBase {
		[SerializeField]
		private Sprite _illustrationSprite;

		public override void DoOperation(UIAVG avg) {
			avg.illustration.sprite = _illustrationSprite;
		}
	}
}