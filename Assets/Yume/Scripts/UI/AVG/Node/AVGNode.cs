using DG.Tweening;
using UnityEngine;
using XNode;
using Yume.Audio;
using Yume.UI;

namespace Yume.AVG {
	public abstract class AVGNodeBase : Node {
		[Input(ShowBackingValue.Never)]
		public int last;
		[Output(ShowBackingValue.Never)]
		public int next;
		protected AVGGraph avgGraph;

		protected override void Init() {
			base.Init();

			avgGraph = graph as AVGGraph;
		}

		public AVGNodeBase GetNextAVGNode() {
			var connection = GetOutputPort(nameof(next)).Connection;
			if (connection != null) {
				return connection.node as AVGNodeBase;
			}
			return null;
		}

		public abstract void DoOperation(UIAVG avg);
	}

	public abstract class AVGFadeOperation : AVGNodeBase {
		[SerializeField]
		private bool _block = true;
		[SerializeField]
		private float _fadeTime;
		[Output(ShowBackingValue.Never)]
		public int onFadeCompleted;

		public override void DoOperation(UIAVG avg) {
			if (_block) {
				avgGraph.block++;
			}
			Tween tween = DOTween(avg, _fadeTime);
			tween.OnComplete(() => {
				avgGraph.tweenList.Remove(tween);
				OnFadeCompleted(avg);
			});
			avgGraph.tweenList.Add(tween);
		}

		protected abstract Tween DOTween(UIAVG avg, float fadeTime);

		protected void OnFadeCompleted(UIAVG avg) {
			var connections = GetOutputPort(nameof(onFadeCompleted)).GetConnections();
			foreach (var connection in connections) {
				(connection.node as AVGNodeBase)?.DoOperation(avg);
			}
			if (_block) {
				avgGraph.block--;
			}
		}
	}

	public class AVGNodes : AVGNodeBase {
		[Output]
		public AVGNodeBase avgNodes;

		public override void DoOperation(UIAVG avg) {
			var connections = GetOutputPort(nameof(avgNodes)).GetConnections();
			foreach (var connection in connections) {
				(connection.node as AVGNodeBase)?.DoOperation(avg);
			}
		}
	}

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

	public class SetBackground : AVGNodeBase {
		[SerializeField]
		private Sprite _bgSprite;

		public override void DoOperation(UIAVG avg) {
			avg.bgImage.sprite = _bgSprite;
		}
	}

	public class SetIllustration : AVGNodeBase {
		[SerializeField]
		private Sprite _illustrationSprite;

		public override void DoOperation(UIAVG avg) {
			avg.illustration.sprite = _illustrationSprite;
		}
	}

	public class PlayBgm : AVGNodeBase {
		[SerializeField]
		private AudioClip _bgmClip;

		public override void DoOperation(UIAVG avg) {
			AudioManager.PlayBgm(_bgmClip);
		}
	}

	public class PlaySound : AVGNodeBase {
		[SerializeField]
		private AudioClip _soundClip;

		public override void DoOperation(UIAVG avg) {
			AudioManager.PlaySound(_soundClip);
		}
	}

	public class SetDialogueVisible : AVGFadeOperation {
		[SerializeField]
		private bool _visible;

		protected override Tween DOTween(UIAVG avg, float fadeTime) {
			float alpha = _visible ? 1.0f : 0;
			return avg.dialogueCanvasGroup.DOFade(alpha, fadeTime);
		}
	}

	public class SetIllustrationVisible : AVGFadeOperation {
		[SerializeField]
		private bool _visible;

		protected override Tween DOTween(UIAVG avg, float fadeTime) {
			float alpha = _visible ? 1.0f : 0;
			return avg.illustration.DOFade(alpha, fadeTime);
		}
	}

	public class SetBackgroundColor : AVGFadeOperation {
		[SerializeField]
		private Color _color;

		protected override Tween DOTween(UIAVG avg, float fadeTime) {
			return avg.bgImage.DOColor(_color, fadeTime);
		}
	}
}