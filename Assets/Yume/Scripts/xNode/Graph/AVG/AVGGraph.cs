using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using XNode;
using Yume.UI;

namespace Yume.AVG {
  [CreateAssetMenu]
  public class AVGGraph : NodeGraph {
    [NonSerialized]
    public int block;
    [NonSerialized]
    public AVGNodeBase avgNode;
    [NonSerialized]
    public List<Tween> tweenList = new List<Tween>();

    public void Enter(UIAVG avg) {
      var node = nodes.Find(n => n is Enter);
      (node as AVGNodeBase)?.Run(avg);
    }

    public void Run(UIAVG avg) {
      if (block > 0) {
        return;
      }

      if (tweenList.Count > 0) {
        while (tweenList.Count > 0) {
          tweenList.First().Complete();
        }
        return;
      }

      avgNode?.Run(avg);
    }

    public void CleanUp() {
      if (tweenList.Count > 0) {
        foreach (var tween in tweenList) {
          tween?.Kill();
        }
        tweenList.Clear();
      }
      avgNode = null;
      block = 0;
    }
  }
}