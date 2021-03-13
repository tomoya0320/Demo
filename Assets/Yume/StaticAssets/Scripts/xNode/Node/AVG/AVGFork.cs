using System;
using DG.Tweening;
using UnityEngine;
using Yume.Data;
using Yume.UI;

namespace Yume.AVG {
  public class AVGFork : AVGNodeBase {
    [Input(ShowBackingValue.Never)]
    public int last;
    [TextArea, Output(ShowBackingValue.Always, ConnectionType.Override)]
    public string option1;
    [TextArea, Output(ShowBackingValue.Always, ConnectionType.Override)]
    public string option2;
    [TextArea, Output(ShowBackingValue.Always, ConnectionType.Override)]
    public string option3;

    public override void Run(UIAVG avg) {
      _SetOption(avg, 0, option1);
      _SetOption(avg, 1, option2);
      _SetOption(avg, 2, option3);
      _OpenOptions(avg);
    }

    private void _SetOption(UIAVG avg, int index, string option) {
      if (string.IsNullOrEmpty(option)) {
        avg.optionBtns[index].gameObject.SetActiveEx(false);
      } else {
        avg.optionBtns[index].gameObject.SetActiveEx(true);
        avg.optionBtns[index].textComponent.text = option;
        avg.optionBtns[index].onClick.RemoveAllListeners();
        avg.optionBtns[index].onClick.AddListener(
          () => {
            avg.historyDialogues += $"\n\n[{DataManager.playerName}]:{option}";
            _CloseOptions(avg, () => _Next(avg, $"option{index + 1}"));
          }
        );
      }
    }

    private void _Next(UIAVG avg, string portName) {
      avgGraph.block--;
      var connection = GetOutputPort(portName).Connection;
      avgGraph.avgNode = connection.node as AVGNodeBase;
      avgGraph.Run(avg);
    }

    private void _OpenOptions(UIAVG avg) {
      avgGraph.block++;
      avg.optionCanvasGroup.DOFade(1.0f, 0.2f).OnComplete(() => avg.optionCanvasGroup.blocksRaycasts = true);
    }

    private void _CloseOptions(UIAVG avg, Action onCompleted) {
      avg.optionCanvasGroup.blocksRaycasts = false;
      avg.optionCanvasGroup.DOFade(0, 0.2f).OnComplete(() => onCompleted());
    }
  }
}