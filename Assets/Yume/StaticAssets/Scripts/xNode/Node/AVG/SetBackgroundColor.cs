using DG.Tweening;
using UnityEngine;

namespace Yume.AVG {
  public class SetBackgroundColor : FadeNodeBase {
    [SerializeField]
		private Color _color;

		protected override Tween DoTween(float fadeTime) {
			return avg.bgImage.DOColor(_color, fadeTime);
		}
	}
}