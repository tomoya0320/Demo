using UnityEngine;

namespace Yume.AVG {
  public class SetBackground : LeafNodeBase {
    [SerializeField]
		private Sprite _bgSprite;

		public override void Run() {
			avg.bgImage.sprite = _bgSprite;
		}
	}
}