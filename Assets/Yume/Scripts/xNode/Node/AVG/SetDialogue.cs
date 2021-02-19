using DG.Tweening;
using UnityEngine;
using Yume.UI;

namespace Yume.AVG {
	public class SetDialogue : FadeNodeBase {
		[Input(ShowBackingValue.Never)]
		public int node;
		[SerializeField]
		private string _name;
		[SerializeField, TextArea]
		private string _dialogue;

    protected override void Init() {
      base.Init();

			block = false;
    }

    protected override Tween DoTween(UIAVG avg, float fadeTime) {
			avg.nameText.text = _name;
			avg.dialogueText.text = StringMap.EMPTY;
			return avg.dialogueText.DOText(_dialogue, fadeTime);
		}
	}
}