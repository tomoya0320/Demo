using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Yume.Battle {
  public static class NumFactory {
    private const float NUM_SHOW_TIME = 0.4f;
    private const float ANIM_DURATION = 0.3f;
    private static GameObject s_numPrototype;
    private static Stack<Text> s_numStack = new Stack<Text>();
    public static Transform numFolder { private set; get; }

    public static void Init(ObjectContainer objectContainer) {
      numFolder = objectContainer.GetTransform("numFolder");
      s_numPrototype = ResourceManager.Load<GameObject>("num_prototype");
    }

    /// <summary>
    /// 显示数字(伤害、治疗等数值)
    /// </summary>
    /// <param name="value"></param>
    /// <param name="pos"></param>
    /// <param name="color"></param>
    public static void ShowNum(int value, Vector3 pos, Color color) {
      if (s_numPrototype == null) {
        return;
      }

      Text num;
      if (s_numStack.Count > 0) {
        num = s_numStack.Pop();
      } else {
        num = ObjectUtil.Instantiate<Text>(s_numPrototype, numFolder);
      }

      num.text = value.ToString();
      color.a = 0;
      num.color = color;
      num.transform.position = pos;
      num.DOFade(1.0f, ANIM_DURATION);
      var rectTransform = num.gameObject.GetRectTransform();
      var width = rectTransform.rect.width;
      var height = rectTransform.rect.height;
      rectTransform.anchoredPosition += 0.5f * new Vector2(Random.Range(-width, width), Random.Range(-height, height));
      rectTransform.DOAnchorPosY(rectTransform.anchoredPosition.y + height, ANIM_DURATION).OnComplete(
        () => {
          // 这里这样判断是因为动画播放过程中战斗结束了数字会被销毁 下同
          if (num != null) {
            num.DOFade(0, ANIM_DURATION).SetDelay(NUM_SHOW_TIME).OnComplete(
              () => {
                if (num != null) {
                  s_numStack.Push(num);
                }
              }
            );
          }
        }
      );
    }

    public static void CleanUp() {
      for (int i = numFolder.childCount - 1; i >= 0; i--) {
        Object.Destroy(numFolder.GetChild(i).gameObject);
      }
      s_numPrototype = null;
      numFolder = null;
      s_numStack.Clear();
    }
  }
}