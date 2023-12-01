using System;
using DG.Tweening;
using UnityEngine;

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

    public override void Run() {
      _SetOption(0, option1);
      _SetOption(1, option2);
      _SetOption(2, option3);
      _OpenOptions();
    }

    private void _SetOption(int index, string option) {
      if (string.IsNullOrEmpty(option)) {
        avg.optionBtns[index].gameObject.SetActiveEx(false);
      } else {
        avg.optionBtns[index].gameObject.SetActiveEx(true);
        avg.optionBtns[index].textComponent.text = option;
        avg.optionBtns[index].onClick.RemoveAllListeners();
        avg.optionBtns[index].onClick.AddListener(
          () => {
            avg.historyDialogues += $"\n\n[{DataManager.playerName}]:{option}";
            _CloseOptions( () => _Next($"option{index + 1}"));
          }
        );
      }
    }

    private void _Next(string portName) {
      avgGraph.block--;
      var connection = GetOutputPort(portName).Connection;
      avgGraph.avgNode = connection.node as AVGNodeBase;
      avgGraph.Run();
    }

    private void _OpenOptions() {
      avgGraph.block++;
      avg.optionCanvasGroup.DOFade(1.0f, 0.2f).OnComplete(
        () => avg.optionCanvasGroup.blocksRaycasts = true
      );
    }

    private void _CloseOptions(Action onCompleted) {
      avg.optionCanvasGroup.blocksRaycasts = false;
      avg.optionCanvasGroup.DOFade(0, 0.2f).OnComplete(() => onCompleted());
    }
  }
}