using System;
using DG.Tweening;
using UnityEngine.UI;
using Yume.Data;
using Yume.UI;

namespace Yume.AVG {
  public class InputName : LeafNodeBase {
    public override void Run(UIAVG avg) {
      avg.inputFieldOkBtn.onClick.RemoveAllListeners();
      avg.inputFieldOkBtn.onClick.AddListener(
        () => {
          // 避免闭包引用到avg对象
          string name = avg.inputField.text;
          if (!string.IsNullOrEmpty(name)) {
            Action action = () => DataManager.AlterName(name);
            avg.onQuitAVG = action + avg.onQuitAVG;
            _CloseInputField(avg);
          }
        }
      );
      ((Text)avg.inputField.placeholder).text = "请输入你的名字";

      _OpenInputField(avg);
    }

    private void _OpenInputField(UIAVG avg) {
      avgGraph.block++;
      avg.inputFieldCanvasGroup.DOFade(1.0f, 0.2f).OnComplete(() => avg.inputFieldCanvasGroup.blocksRaycasts = true);
    }

    private void _CloseInputField(UIAVG avg) {
      avg.inputFieldCanvasGroup.blocksRaycasts = false;
      avg.inputFieldCanvasGroup.DOFade(0, 0.2f).OnComplete(
        () => {
          avgGraph.block--;
          avgGraph.Run(avg);
        }
      );
    }
  }
}