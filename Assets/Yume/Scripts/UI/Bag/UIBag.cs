using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Yume.Broadcast;
using Yume.Data;
using Yume.RedPointSystem;
using Yume.Resource;
using EventType = Yume.Broadcast.EventType;

namespace Yume.UI {
  public class UIBag : UIBase {
    private class UIItemTerm : UIChildBase, IScrollTerm<ItemData> {
      private ItemData m_item;
      private Text m_numText;
      private Image m_iconImg;
      private Button m_clickButton;
      private UIItemGroup m_itemGroup;
      private UIRedPoint m_itemRedPoint;
      public bool isValid => m_item != null;

      public UIItemTerm(GameObject root, UIItemGroup itemGroup) : base(root) {
        m_numText = GetComponent<Text>("NumText");
        m_iconImg = GetComponent<Image>("IconImg");
        m_clickButton = GetComponent<Button>("ClickButton");

        var itemRedPointRoot = root.FindGo(StringMap.UI_RED_POINT);
        m_itemRedPoint = new UIRedPoint(RedPointTreeType.MAIN_ITEM, m_item?.id, MainItemRedPointTree.MAIN_ITEM, itemRedPointRoot);

        m_itemGroup = itemGroup;

        _AddListener();
      }

      private void _AddListener() {
        m_clickButton.onClick.AddListener(() => {
          if (!m_item.isViewed) {
            DataManager.SetItemViewed(m_item);
          }
          UIManager.OpenOne("item_detail", null, m_item);
        });
      }

      public void UpdateScrollTerm(ItemData item) {
        m_item = item;

        m_itemRedPoint.Update(m_item?.id);
        gameObject.SetActiveEx(m_item != null);

        if (m_item != null) {
          m_numText.text = StringUtil.IntegerToString(m_item.count);
          m_iconImg.gameObject.SetActiveEx(true);
          m_iconImg.sprite = ResourceManager.Load<Sprite>(ResourceManager.Load<ItemTemplate>(m_item.id).iconFileName);
        }
      }

      public override void Destroy() {
        m_itemRedPoint.Destroy();
      }
    }

    private class UIItemGroup : UIChildBase {
      private List<ItemData> m_itemList;
      private ScrollRect m_itemScrollRect;
      private UIScrollView<ItemData, UIItemTerm> m_itemScrollView;
      public UIScrollView<ItemData, UIItemTerm> itemScrollView => m_itemScrollView;
      private List<UIItemTerm> m_itemTermList = new List<UIItemTerm>();

      public UIItemGroup(GameObject root) : base(root) {
        m_itemScrollRect = GetComponent<ScrollRect>("ItemScrollRect");

        m_itemList = DataManager.userItems;

        for (int i = 0; i < m_itemScrollRect.content.childCount; i++) {
          m_itemTermList.Add(new UIItemTerm(m_itemScrollRect.content.GetChild(i).gameObject, this));
        }

        m_itemScrollView = new UIScrollView<ItemData, UIItemTerm>(m_itemScrollRect, m_itemList, m_itemTermList);

        EventCenter.AddListener(EventType.UPDATE_ITEM_INFO, _UpdateScrollView);
      }

      public override void Destroy() {
        foreach (var itemTerm in m_itemTermList) {
          itemTerm.Destroy();
        }

        EventCenter.RemoveListener(EventType.UPDATE_ITEM_INFO, _UpdateScrollView);
      }

      private void _UpdateScrollView() {
        m_itemScrollView.UpdateView();
      }

      public override void OnUpdate() {
        m_itemScrollView.OnUpdate();
      }
    }

    private UIItemGroup m_itemGroup;

    public override void OnInit(UITemplate uiTemplate, GameObject root) {
      base.OnInit(uiTemplate, root);

      m_itemGroup = new UIItemGroup(GetGameObject("ItemGroup"));

      _AddListener();
    }

    public override void OnUpdate() {
      m_itemGroup.OnUpdate();
    }

    private void _AddListener() {
      GetComponent<Button>("close").onClick.AddListener(() => UIManager.CloseOne(name));
    }

    public override void Destroy() {
      base.Destroy();
      m_itemGroup.Destroy();
    }
  }
}