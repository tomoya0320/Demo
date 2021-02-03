using DG.Tweening;
using UnityEngine;
using XNode;
using Yume.UI;

namespace Yume.AVG {
  public abstract class AVGNodeBase : Node {
		protected AVGGraph avgGraph;

		protected override void Init() {
			base.Init();

			avgGraph = graph as AVGGraph;
		}

		public abstract void Run(UIAVG avg);
	}

	public abstract class FadeNodeBase : AVGNodeBase {
		[SerializeField]
		protected bool block = true;
		[SerializeField]
		protected float fadeTime;
		[Output(ShowBackingValue.Never)]
		public int onFadeCompleted;

		public override void Run(UIAVG avg) {
			if (block) {
				avgGraph.block++;
			}
			Tween tween = DOTween(avg, fadeTime);
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
				(connection.node as AVGNodeBase)?.Run(avg);
			}
			if (block) {
				avgGraph.block--;
			}
		}
	}
}