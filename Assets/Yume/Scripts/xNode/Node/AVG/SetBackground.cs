using UnityEngine;
using Yume.UI;

namespace Yume.AVG {
  public class SetBackground : AVGNodeBase {
		[SerializeField]
		private Sprite _bgSprite;

		public override void DoOperation(UIAVG avg) {
			avg.bgImage.sprite = _bgSprite;
		}
	}
}