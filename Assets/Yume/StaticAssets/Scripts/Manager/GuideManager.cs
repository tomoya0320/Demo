using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Yume.UI;
using Object = UnityEngine.Object;

namespace Yume {
  public static class GuideManager {
    public static bool isGuiding { private set; get; }
    
    private class UIGuideContent {
      public Image icon;
      public Text description;
      public GameObject gameObject;
      public RectTransform rectTransform;

      public UIGuideContent(GameObject gameObject) {
        var objectContainer = gameObject.GetComponent<ObjectContainer>();
        this.gameObject = gameObject;
        rectTransform = gameObject.GetRectTransform();
        icon = objectContainer.GetComponent<Image>("icon");
        description = objectContainer.GetComponent<Text>("description");
      }
    }

    private static GameObject s_guidMask;
    private static GameObject s_indicator;
    private static YButton s_guidMaskButton;
    private static UIGuideContent s_uiGuideContent;

    public static void Init() {
      var guideMaskPrefab = Resources.Load<GameObject>("guide_mask");
      s_guidMask = Object.Instantiate(guideMaskPrefab, UIManager.guidelCanvas);
      s_guidMask.name = guideMaskPrefab.name;
      s_guidMaskButton = s_guidMask.GetComponent<YButton>();
      s_guidMask.SetActiveEx(false);

      var guideContentPrefab = Resources.Load<GameObject>("guide_content");
      var guideContent = Object.Instantiate(guideContentPrefab, UIManager.guidelCanvas);
      guideContent.name = guideContentPrefab.name;
      s_uiGuideContent = new UIGuideContent(guideContent);
      guideContent.SetActiveEx(false);

      var indicatorPrefab = Resources.Load<GameObject>("indicator");
      s_indicator = Object.Instantiate(indicatorPrefab, UIManager.guidelCanvas);
      s_indicator.name = indicatorPrefab.name;
      s_indicator.SetActiveEx(false);
    }

    // 被引导的UI目前仅限NORMAL层
    public static void ShowGuide(GuideData guide) {
      isGuiding = true;
      // 引导过程中需要屏蔽回退响应
      UIManager.activeBack--;
      var guideTemplate = ResourceManager.Load<GuideTemplate>(guide.id);
      var stepEnumerator = new YEnumerator<GuideStep>(guideTemplate.guideSteps);
      CoroutineHelper.instance.Run(_ShowGuide(guide, stepEnumerator));
    }

    private static IEnumerator _ShowGuide(GuideData guide, YEnumerator<GuideStep> stepEnumerator) {
      if (stepEnumerator.TryMoveNext()) {
        // 等UI动画结束了再调用
        yield return new WaitWhile(UIManager.IsAnyBlocked);
        if (UIManager.GetTopUI(UIType.NORMAL).name == stepEnumerator.current.uiName) {
          _SetGuideStep(guide, stepEnumerator);
        } else {
          UIManager.OpenOne(stepEnumerator.current.uiName, () => _SetGuideStep(guide, stepEnumerator));
        }
      } else {
        isGuiding = false;
        UIManager.activeBack++;
        DataManager.CompleteGuide(guide);
      }
    }

    private static void _SetGuideStep(GuideData guide, YEnumerator<GuideStep> stepEnumerator) {
      // 找到目标UI对象
      var ui = UIManager.GetTopUI(UIType.NORMAL);
      RectTransform dynamicListObj;
      if (stepEnumerator.current.isDynamicListObj) {
        dynamicListObj = ui.GetDynamicListObj(stepEnumerator.current.targetObj, stepEnumerator.current.dynamicListObjId);
      } else {
        dynamicListObj = GameObject.Find(stepEnumerator.current.targetObj).GetComponent<RectTransform>();
      }
      float width = dynamicListObj.rect.width;
      float height = dynamicListObj.rect.height;
      Vector2 targetPos = Camera.main.WorldToScreenPoint(dynamicListObj.position);
      Vector4 clipRect = new Vector4(targetPos.x - 0.5f * width, targetPos.y - 0.5f * height, targetPos.x + 0.5f * width, targetPos.y + 0.5f * height);
      // 显示指示器
      s_indicator.GetComponent<RectTransform>().sizeDelta = dynamicListObj.rect.size;
      s_indicator.transform.position = dynamicListObj.position;
      s_indicator.SetActiveEx(true);
      // 归一化的坐标
      Vector4 clipRectNormalized = new Vector4(clipRect.x / Screen.width, clipRect.y / Screen.height, clipRect.z / Screen.width, clipRect.w / Screen.height);
      // 设置遮罩
      s_guidMaskButton.targetGraphic.material.SetVector("_CustomRect", clipRectNormalized);
      s_guidMask.SetActiveEx(true);
      // 设置引导位置以及npc图标和对话内容
      width = s_uiGuideContent.rectTransform.rect.width;
      height = s_uiGuideContent.rectTransform.rect.height;
      Vector2 contentPos;
      switch (stepEnumerator.current.positionType) {
        case PositionType.LEFT_TOP:
          contentPos = new Vector2(-0.5f * Screen.width, 0.5f * Screen.height - height);
          break;
        case PositionType.LEFT_BOTTOM:
          contentPos = new Vector2(-0.5f * Screen.width, -0.5f * Screen.height);
          break;
        case PositionType.RIGHT_TOP:
          contentPos = new Vector2(0.5f * Screen.width - width, 0.5f * Screen.height - height);
          break;
        case PositionType.RIGHT_BOTTOM:
          contentPos = new Vector2(0.5f * Screen.width - width, -0.5f * Screen.height);
          break;
        default:
          contentPos = Vector2.zero;
          break;
      }
      s_uiGuideContent.rectTransform.anchoredPosition = contentPos;
      s_uiGuideContent.icon.SetSpriteAsync(stepEnumerator.current.iconFileName);
      s_uiGuideContent.description.text = StringMap.EMPTY;
      s_uiGuideContent.description.DOText(stepEnumerator.current.description, 15.0f).SetSpeedBased().OnComplete(
        () =>
          s_guidMaskButton.onClickWithPointer = pointer => {
            if (stepEnumerator.current.ignoreClickPos || MathUtil.IsPointInRect(pointer.position, clipRect)) {
              dynamicListObj.GetComponent<Button>()?.OnPointerClick(pointer);

              s_uiGuideContent.gameObject.SetActiveEx(false);
              s_guidMask.SetActiveEx(false);
              s_indicator.SetActiveEx(false);

              s_guidMaskButton.onClickWithPointer = null;

              // 执行下一步引导
              CoroutineHelper.instance.Run(_ShowGuide(guide, stepEnumerator));
            }
          }
      );
      s_uiGuideContent.gameObject.SetActiveEx(true);
    }
  }
}