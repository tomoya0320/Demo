using DG.Tweening;
using UnityEngine;
using XNode;
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

    public override object GetValue(NodePort port) {
			// 暂时没用
      return null;
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
}