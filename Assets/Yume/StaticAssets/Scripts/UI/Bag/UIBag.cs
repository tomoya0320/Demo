using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Yume.Broadcast;
using EventType = Yume.Broadcast.EventType;

namespace Yume.UI {
  public class UIBag : UIGaussianBlurBg {
    private class UIItemTerm : UIChildBase, IScrollTerm<ItemData> {
      private Text m_numText;
      private Image m_iconImg;
      private ItemData m_item;
      public bool isValid => m_item != null;

      public UIItemTerm(GameObject root) : base(root) {
        m_numText = GetComponent<Text>("NumText");
        m_iconImg = GetComponent<Image>("IconImg");

        _AddListener();
      }

      private void _AddListener() {
        GetComponent<Button>("ClickButton").onClick.AddListener(() => {
          UIManager.OpenOne("item_detail", null, m_item);
        });
      }

      public void UpdateScrollTerm(ItemData item) {
        m_item = item;

        gameObject.SetActiveEx(m_item != null);

        if (m_item != null) {
          m_numText.text = StringUtil.IntegerToString(m_item.count);
          m_iconImg.SetSprite(ResourceManager.Load<ItemTemplate>(m_item.id).iconFileName);
        }
      }

      public IScrollTerm<ItemData> Clone() {
        var clone = ObjectUtil.Instantiate(gameObject, gameObject.GetParent());
        return new UIItemTerm(clone);
      }
    }

    private class UIItemContainer : UIChildBase, IScrollContainer<ItemData, UIItemTerm> {
      private List<ItemData> m_itemList;
      private ScrollRect m_itemScrollRect;
      public List<ItemData> dataList => m_itemList;
      public List<UIItemTerm> viewList => m_itemTermList;
      private UIScrollView<ItemData, UIItemTerm> m_itemScrollView;
      private List<UIItemTerm> m_itemTermList = new List<UIItemTerm>();
      private Predicate<ItemData> m_findDisplayItem = item => !ResourceManager.Load<ItemTemplate>(item.id).hideDisplay;

      public UIItemContainer(GameObject root) : base(root) {
        m_itemScrollRect = GetComponent<ScrollRect>("ItemScrollRect");
        
        m_itemList = DataManager.playerItems.FindAll(m_findDisplayItem);
        
        for (int i = 0; i < m_itemScrollRect.content.childCount; i++) {
          m_itemTermList.Add(new UIItemTerm(m_itemScrollRect.content.GetChild(i).gameObject));
        }

        m_itemScrollView = new UIScrollView<ItemData, UIItemTerm>(m_itemScrollRect, this);

        EventCenter.AddListener(EventType.UPDATE_ITEM_INFO, _RefreshScrollView);
      }

      public override void Destroy() {
        foreach (var itemTerm in m_itemTermList) {
          itemTerm.Destroy();
        }

        EventCenter.RemoveListener(EventType.UPDATE_ITEM_INFO, _RefreshScrollView);
      }

      private void _RefreshScrollView() {
        m_itemList = DataManager.playerItems.FindAll(m_findDisplayItem);
        m_itemScrollView.UpdateView();
      }

      public override void OnUpdate() {
        m_itemScrollView.OnUpdate();
      }

      public override void OnOpen() {
        m_itemList.Sort((left, right) => string.CompareOrdinal(left.id, right.id));
        m_itemScrollView.ResetNormalizedPos();
      }
    }

    private UIItemContainer m_itemContainer;

    public override void OnInit(UITemplate uiTemplate, GameObject gameObject) {
      base.OnInit(uiTemplate, gameObject);

      m_itemContainer = new UIItemContainer(GetGameObject("ItemContainer"));

      _AddListener();
    }

    public override void Open(Action callback = null, params object[] args) {
      base.Open(callback, args);
      
      m_itemContainer.OnOpen();
    }

    public override void OnUpdate() {
      m_itemContainer.OnUpdate();
    }

    private void _AddListener() {
      GetComponent<Button>("close").onClick.AddListener(() => UIManager.CloseOne(name));
    }

    public override void Destroy() {
      base.Destroy();
      m_itemContainer.Destroy();
    }
  }
}