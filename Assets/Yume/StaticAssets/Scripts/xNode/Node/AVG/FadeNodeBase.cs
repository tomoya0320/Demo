using DG.Tweening;
using UnityEngine;

namespace Yume.AVG {
  public abstract class FadeNodeBase : LeafNodeBase {
    [SerializeField]
    protected bool block = true;
    [SerializeField]
    protected float fadeTime;
    [Output(ShowBackingValue.Never)]
    public int onFadeCompleted;

    public override void Run() {
      if (block) {
        avgGraph.block++;
      }
      Tween tween = DoTween(fadeTime);
      tween.OnComplete(() => {
        avgGraph.tweens.Remove(tween);
        OnFadeCompleted();
      });
      avgGraph.tweens.Add(tween);
    }

    protected abstract Tween DoTween(float fadeTime);

    protected void OnFadeCompleted() {
      var connections = GetOutputPort(nameof(onFadeCompleted)).GetConnections();
      foreach (var connection in connections) {
        (connection.node as AVGNodeBase)?.Run();
      }
      if (block) {
        avgGraph.block--;
      }
    }
  }
}