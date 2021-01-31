using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using XNode;
using Yume.UI;

namespace Yume.AVG {
  [CreateAssetMenu]
  public class AVGGraph : NodeGraph {
    public int block;
    private AVGNodeBase m_avgNode;
    public List<Tween> tweenList = new List<Tween>();

    public AVGGraph Init() {
      var enter = nodes.Find(n => n is EnterNode);
      if(enter != null) {
        m_avgNode = (enter as EnterNode).GetFirstAVGNode();
      }
      return this;
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

      if (m_avgNode == null) {
        _CleanUp();
        avg.QuitAVG();
      } else {
        m_avgNode.DoOperation(avg);
        m_avgNode = m_avgNode.GetNextAVGNode();
      }
    }

    private void _CleanUp() {
      if (tweenList.Count > 0) {
        foreach (var tween in tweenList) {
          tween?.Kill();
        }
        tweenList.Clear();
      }

      block = 0;
    }
  }
}