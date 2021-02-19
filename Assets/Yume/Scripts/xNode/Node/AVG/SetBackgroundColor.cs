using DG.Tweening;
using UnityEngine;
using Yume.UI;

namespace Yume.AVG {
  public class SetBackgroundColor : FadeNodeBase {
		[Input(ShowBackingValue.Never)]
		public int node;
		[SerializeField]
		private Color _color;

		protected override Tween DoTween(UIAVG avg, float fadeTime) {
			return avg.bgImage.DOColor(_color, fadeTime);
		}
	}
}