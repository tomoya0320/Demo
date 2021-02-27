using DG.Tweening;
using UnityEngine;
using Yume.UI;

namespace Yume.AVG {
	public class SetDialogueVisible : FadeNodeBase {
    [SerializeField]
		private bool _visible;

		protected override Tween DoTween(UIAVG avg, float fadeTime) {
			float alpha = _visible ? 1.0f : 0;
			return avg.dialogueCanvasGroup.DOFade(alpha, fadeTime);
		}
	}
}