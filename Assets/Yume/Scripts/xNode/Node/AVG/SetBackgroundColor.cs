using DG.Tweening;
using UnityEngine;
using Yume.UI;

namespace Yume.AVG {
  public class SetBackgroundColor : AVGFadeOperation {
		[SerializeField]
		private Color _color;

		protected override Tween DOTween(UIAVG avg, float fadeTime) {
			return avg.bgImage.DOColor(_color, fadeTime);
		}
	}
}