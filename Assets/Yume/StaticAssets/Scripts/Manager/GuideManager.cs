using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Yume.UI;
using Object = UnityEngine.Object;

namespace Yume {
  public static class GuideManager {
    private class UIGuideContent {
      public Image icon;
      public Text description;
      public GameObject gameObject;

      public UIGuideContent(GameObject gameObject) {
        var objectContainer = gameObject.GetComponent<ObjectContainer>();
        this.gameObject = gameObject;
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
      CoroutineHelper.instance.Run(_ShowGuide(guide));
    }

    private static IEnumerator _ShowGuide(GuideData guide) {
      // 等UI动画结束了再调用
      yield return new WaitWhile(UIManager.IsAnyBlocked);
      var guideTemplate = ResourceManager.Load<GuideTemplate>(guide.id);
      Action action = () => {
        // 找到目标UI对象
        var uiRoot = UIManager.GetTopUI(UIType.NORMAL).root;
        var rectTransform = uiRoot.FindGo(guideTemplate.content.targetUIObject).GetComponent<RectTransform>();
        float width = rectTransform.rect.width;
        float height = rectTransform.rect.height;
        Vector2 targetPos = Camera.main.WorldToScreenPoint(rectTransform.position);
        Vector4 clipRect = new Vector4(targetPos.x - 0.5f * width, targetPos.y - 0.5f * height, targetPos.x + 0.5f * width, targetPos.y + 0.5f * height);
        // 显示指示器
        s_indicator.GetComponent<RectTransform>().sizeDelta = rectTransform.rect.size;
        s_indicator.transform.position = rectTransform.position;
        s_indicator.SetActiveEx(true);
        // 归一化的坐标
        Vector4 clipRectNormalized = new Vector4(clipRect.x / Screen.width, clipRect.y / Screen.height, clipRect.z / Screen.width, clipRect.w / Screen.height);
        // 设置遮罩
        s_guidMaskButton.targetGraphic.material.SetVector("_CustomRect", clipRectNormalized);
        s_guidMask.SetActiveEx(true);

        s_uiGuideContent.icon.sprite = ResourceManager.Load<Sprite>(guideTemplate.content.iconFileName);
        s_uiGuideContent.description.text = StringMap.EMPTY;
        s_uiGuideContent.description.DOText(guideTemplate.content.description, 15.0f).SetSpeedBased().OnComplete(
          () => {
            s_guidMaskButton.onClickWithPointer = pointer => {
              if (guideTemplate.ignoreClickPos || MathUtil.IsPointInRect(pointer.position, clipRect)) {
                rectTransform.GetComponent<Button>()?.OnPointerClick(pointer);

                s_uiGuideContent.gameObject.SetActiveEx(false);
                s_guidMask.SetActiveEx(false);
                s_indicator.SetActiveEx(false);

                s_guidMaskButton.onClickWithPointer = null;

                CoroutineHelper.instance.Run(_CompleteGuid(guide));
              }
            };
          }
        );
        s_uiGuideContent.gameObject.SetActiveEx(true);
      };
      if (UIManager.GetTopUI(UIType.NORMAL).name == guideTemplate.content.targetUI) {
        action();
      } else {
        UIManager.OpenOne(guideTemplate.content.targetUI, action);
      }
    }

    private static IEnumerator _CompleteGuid(GuideData guide) {
      // 等UI动画结束了再调用
      yield return new WaitWhile(UIManager.IsAnyBlocked);
      DataManager.CompleteGuide(guide);
    }
  }
}