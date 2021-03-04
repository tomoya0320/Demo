using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Yume.Battle {
  public static class NumFactory {
    private const float NUM_SHOW_TIME = 0.4f;
    private const float ANIM_DURATION = 0.3f;
    private static Text s_numPrototype;
    private static Stack<Text> s_numStack = new Stack<Text>();
    public static Transform numFolder { private set; get; }

    public static void Init(ObjectContainer objectContainer) {
      numFolder = objectContainer.GetTransform("numFolder");
      s_numPrototype = numFolder.GetComponentInChildren<Text>(true);
      s_numPrototype.gameObject.SetActiveEx(false);
    }

    /// <summary>
    /// 显示数字(伤害、治疗等数值)
    /// </summary>
    /// <param name="value"></param>
    /// <param name="pos"></param>
    /// <param name="color"></param>
    public static void ShowNum(float value, Vector3 pos, Color color) {
      if (s_numPrototype == null) {
        return;
      }

      Text num;
      if (s_numStack.Count > 0) {
        num = s_numStack.Pop();
      } else {
        num = Object.Instantiate(s_numPrototype, numFolder);
      }
      num.gameObject.SetActiveEx(true);
      num.text = $"{value:0}";
      color.a = 0;
      num.color = color;
      num.transform.position = pos;
      num.DOFade(1.0f, ANIM_DURATION);
      var rectTransform = num.gameObject.GetRectTransform();
      rectTransform.DOAnchorPosY(rectTransform.anchoredPosition.y + rectTransform.rect.height, ANIM_DURATION).OnComplete(
        () => {
          num.DOFade(0, ANIM_DURATION).SetDelay(NUM_SHOW_TIME).OnComplete(
            () => {
              num.gameObject.SetActiveEx(false);
              s_numStack.Push(num);
            }
          );
        }
      );
    }

    public static void CleanUp() {
      s_numPrototype = null;
      numFolder = null;
      while (s_numStack.Count > 0) {
        Object.Destroy(s_numStack.Pop().gameObject);
      }
    }
  }
}