using System;
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
      private Text m_numText;
      private Image m_iconImg;
      private ItemData m_item;
      private Button m_clickButton;
      public bool isValid => m_item != null;

      public UIItemTerm(GameObject root) : base(root) {
        m_numText = GetComponent<Text>("NumText");
        m_iconImg = GetComponent<Image>("IconImg");
        m_clickButton = GetComponent<Button>("ClickButton");

        _AddListener();
      }

      private void _AddListener() {
        m_clickButton.onClick.AddListener(() => {
          UIManager.OpenOne("item_detail", null, m_item);
        });
      }

      public void UpdateScrollTerm(ItemData item) {
        m_item = item;

        gameObject.SetActiveEx(m_item != null);

        if (m_item != null) {
          m_numText.text = StringUtil.IntegerToString(m_item.count);
          m_iconImg.sprite = ResourceManager.Load<Sprite>(ResourceManager.Load<ItemTemplate>(m_item.id).iconFileName);
        }
      }
    }

    private class UIItemGroup : UIChildBase, IScrollGroup<ItemData, UIItemTerm> {
      private List<ItemData> m_itemList;
      private ScrollRect m_itemScrollRect;
      public List<ItemData> dataList => m_itemList;
      public List<UIItemTerm> viewList => m_itemTermList;
      private UIScrollView<ItemData, UIItemTerm> m_itemScrollView;
      private Predicate<ItemData> m_findDisplayItem = (item) => {
        return !ResourceManager.Load<ItemTemplate>(item.id).hideDisplay;
      };
      private List<UIItemTerm> m_itemTermList = new List<UIItemTerm>();
      public UIScrollView<ItemData, UIItemTerm> itemScrollView => m_itemScrollView;

      public UIItemGroup(GameObject root) : base(root) {
        m_itemScrollRect = GetComponent<ScrollRect>("ItemScrollRect");

        m_itemList = DataManager.userItems.FindAll(m_findDisplayItem);

        for (int i = 0; i < m_itemScrollRect.content.childCount; i++) {
          m_itemTermList.Add(new UIItemTerm(m_itemScrollRect.content.GetChild(i).gameObject));
        }

        m_itemScrollView = new UIScrollView<ItemData, UIItemTerm>(m_itemScrollRect, this);

        EventCenter.AddListener(EventType.UPDATE_ITEM_INFO, _UpdateScrollView);
      }

      public override void Destroy() {
        foreach (var itemTerm in m_itemTermList) {
          itemTerm.Destroy();
        }

        EventCenter.RemoveListener(EventType.UPDATE_ITEM_INFO, _UpdateScrollView);
      }

      private void _UpdateScrollView() {
        m_itemList = DataManager.userItems.FindAll(m_findDisplayItem);
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