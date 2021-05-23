using System;
using DG.Tweening;
using UnityEngine.UI;

namespace Yume.AVG {
  public class InputName : LeafNodeBase {
    public override void Run() {
      avg.inputFieldOkBtn.onClick.RemoveAllListeners();
      avg.inputFieldOkBtn.onClick.AddListener(
        () => {
          string name = avg.inputField.text;
          if (string.IsNullOrEmpty(name)) {
            return;
          }
          // 注意顺序不能反了
          Action action = () => DataManager.AlterName(name);
          avg.playerName = name;
          avg.onQuitAVG = action + avg.onQuitAVG;
          _CloseInputField();
        }
      );
      ((Text)avg.inputField.placeholder).text = "请输入你的名字";

      _OpenInputField();
    }

    private void _OpenInputField() {
      avgGraph.block++;
      avg.inputFieldCanvasGroup.DOFade(1.0f, 0.2f).OnComplete(
        () => avg.inputFieldCanvasGroup.blocksRaycasts = true
      );
    }

    private void _CloseInputField() {
      avg.inputFieldCanvasGroup.blocksRaycasts = false;
      avg.inputFieldCanvasGroup.DOFade(0, 0.2f).OnComplete(
        () => {
          avgGraph.block--;
          avgGraph.Run();
        }
      );
    }
  }
}