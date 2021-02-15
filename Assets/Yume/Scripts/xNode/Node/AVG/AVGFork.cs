using UnityEngine;
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
      avgGraph.block++;
      avg.dialogOptions.SetActiveEx(true);

      if (string.IsNullOrEmpty(option1)) {
        avg.dialogOptionBtns[0].gameObject.SetActiveEx(false);
      } else {
        avg.dialogOptionBtns[0].gameObject.SetActiveEx(true);
        avg.dialogOptionBtns[0].textComponent.text = option1;
        avg.dialogOptionBtns[0].onClick.RemoveAllListeners();
        avg.dialogOptionBtns[0].onClick.AddListener(() => {
          avgGraph.block--;
          var connection = GetOutputPort(nameof(option1)).Connection;
          avgGraph.avgNode = connection.node as AVGNodeBase;
          avgGraph.Run(avg);
          avg.dialogOptions.SetActiveEx(false);
        });
      }

      if (string.IsNullOrEmpty(option2)) {
        avg.dialogOptionBtns[1].gameObject.SetActiveEx(false);
      } else {
        avg.dialogOptionBtns[1].gameObject.SetActiveEx(true);
        avg.dialogOptionBtns[1].textComponent.text = option2;
        avg.dialogOptionBtns[1].onClick.RemoveAllListeners();
        avg.dialogOptionBtns[1].onClick.AddListener(() => {
          avgGraph.block--;
          var connection = GetOutputPort(nameof(option2)).Connection;
          avgGraph.avgNode = connection.node as AVGNodeBase;
          avgGraph.Run(avg);
          avg.dialogOptions.SetActiveEx(false);
        });
      }

      if (string.IsNullOrEmpty(option3)) {
        avg.dialogOptionBtns[2].gameObject.SetActiveEx(false);
      } else {
        avg.dialogOptionBtns[2].gameObject.SetActiveEx(true);
        avg.dialogOptionBtns[2].textComponent.text = option3;
        avg.dialogOptionBtns[2].onClick.RemoveAllListeners();
        avg.dialogOptionBtns[2].onClick.AddListener(() => {
          avgGraph.block--;
          var connection = GetOutputPort(nameof(option3)).Connection;
          avgGraph.avgNode = connection.node as AVGNodeBase;
          avgGraph.Run(avg);
          avg.dialogOptions.SetActiveEx(false);
        });
      }
    }
  }
}