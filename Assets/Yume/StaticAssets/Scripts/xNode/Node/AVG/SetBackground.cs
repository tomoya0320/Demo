using UnityEngine;
using Yume.UI;

namespace Yume.AVG {
  public class SetBackground : LeafNodeBase {
    [SerializeField]
		private Sprite _bgSprite;

		public override void Run(UIAVG avg) {
			avg.bgImage.sprite = _bgSprite;
		}
	}
}