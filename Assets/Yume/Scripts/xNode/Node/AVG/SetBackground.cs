using UnityEngine;
using Yume.UI;

namespace Yume.AVG {
  public class SetBackground : AVGNodeBase {
		[Input(ShowBackingValue.Never)]
		public int node;
		[SerializeField]
		private Sprite _bgSprite;

		public override void Run(UIAVG avg) {
			avg.bgImage.sprite = _bgSprite;
		}
	}
}