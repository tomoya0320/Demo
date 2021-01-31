using DG.Tweening;
using UnityEngine;
using Yume.UI;

namespace Yume.AVG {
  public class SetDialogue : AVGNodeBase {
		[SerializeField]
		private string _name;
		[SerializeField, TextArea]
		private string _dialogue;

		public override void DoOperation(UIAVG avg) {
			avg.nameText.text = _name;
			avg.dialogueText.text = StringMap.EMPTY;
			Tween tween = avg.dialogueText.DOText(_dialogue, 20.0f).SetSpeedBased();
			tween.OnComplete(() => avgGraph.tweenList.Remove(tween));
			avgGraph.tweenList.Add(tween);
		}
	}
}