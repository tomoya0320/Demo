using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using XNode;
using Yume.UI;

namespace Yume.AVG {
  [CreateAssetMenu]
  public class AVGGraph : NodeGraph {
    public bool disableSkip;
    [NonSerialized]
    public int block;
    [NonSerialized]
    public AVGNodeBase avgNode;
    [NonSerialized]
    public List<Tween> tweens = new List<Tween>();

    public void Enter(UIAVG avg) {
      var node = nodes.Find(n => n is Enter);
      (node as AVGNodeBase)?.Run(avg);
    }

    public void Run(UIAVG avg) {
      if (block > 0) {
        return;
      }

      if (tweens.Count > 0) {
        while (tweens.Count > 0) {
          tweens.First().Complete();
        }
        return;
      }

      avgNode?.Run(avg);
    }

    public void CleanUp() {
      if (tweens.Count > 0) {
        foreach (var tween in tweens) {
          tween?.Kill();
        }
        tweens.Clear();
      }
      avgNode = null;
      block = 0;
    }
  }
}