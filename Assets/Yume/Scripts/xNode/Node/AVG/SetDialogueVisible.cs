using DG.Tweening;
using UnityEngine;
using Yume.UI;

namespace Yume.AVG {
	public class SetDialogueVisible : FadeNodeBase {
		[Input(ShowBackingValue.Never)]
		public int node;
		[SerializeField]
		private bool _visible;

		protected override Tween DOTween(UIAVG avg, float fadeTime) {
			float alpha = _visible ? 1.0f : 0;
			return avg.dialogueCanvasGroup.DOFade(alpha, fadeTime);
		}
	}
}