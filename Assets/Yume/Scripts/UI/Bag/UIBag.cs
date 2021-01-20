using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Yume.Battle;
using Yume.Broadcast;
using Yume.DB;
using Yume.RedPointSystem;
using Yume.Resource;
using Yume.String;
using EventType = Yume.Broadcast.EventType;

namespace Yume.UI {
  public class UIBag : UIBase {
    private class UIItemTerm : UIChildBase, IScrollTerm<ItemDB> {
      private ItemDB m_item;
      private Text m_numText;
      private Image m_iconImg;
      private Button m_clickButton;
      private GameObject m_selected;
      private UIItemGroup m_itemGroup;
      private UIRedPoint m_itemRedPoint;
      public bool isValid => m_item != null;

      public UIItemTerm(GameObject root, UIItemGroup itemGroup) : base(root) {
        m_selected = GetGameObject("Selected");
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
          m_itemGroup.SwitchMail(m_item);
          if (!m_item.isViewed) {
            m_item.isViewed = true;

            RedPointManager.instance.SetRedPointNum(RedPointTreeType.MAIN_ITEM, m_item.id, MainItemRedPointTree.MAIN_ITEM, 0);
          }
        });
      }

      public async void UpdateScrollTerm(ItemDB data) {
        m_item = data;

        m_itemRedPoint.Update(m_item?.id);
        gameObject.SetActiveEx(m_item != null);
        m_selected.SetActiveEx(m_item == m_itemGroup.selectedItem);

        if (m_item != null) {
          m_numText.text = StringUtil.IntegerToString(m_item.num);
          m_iconImg.gameObject.SetActiveEx(true);
          m_iconImg.sprite = await ResourceManager.LoadAssetAsync<Sprite>(
                                   DBManager.instance.GetItemInfo(m_item.id).iconFileName);
        }
      }

      public override void Destroy() {
        m_itemRedPoint.Destroy();
      }
    }

    private class UIItemGroup : UIChildBase {
      private Text m_nameNum;
      private Image m_iconImg;
      private Text m_description;
      private Sprite m_noItemSprite;
      private ItemDB m_selectedItem;
      private List<ItemDB> m_itemList;
      private ScrollRect m_itemScrollRect;
      public ItemDB selectedItem => m_selectedItem;
      private UIScrollView<ItemDB, UIItemTerm> m_itemScrollView;
      public UIScrollView<ItemDB, UIItemTerm> itemScrollView => m_itemScrollView;
      private List<UIItemTerm> m_itemTermList = new List<UIItemTerm>();

      public UIItemGroup(GameObject root) : base(root) {
        m_iconImg = GetComponent<Image>("IconImage");
        m_nameNum = GetComponent<Text>("NameNumText");
        m_description = GetComponent<Text>("DescriptionText");
        m_itemScrollRect = GetComponent<ScrollRect>("ItemScrollRect");

        m_itemList = DBManager.instance.userItems;
        m_noItemSprite = AssetLoader.instance.GetValue<Sprite>("noItemSprite");

        for (int i = 0; i < m_itemScrollRect.content.childCount; i++) {
          m_itemTermList.Add(new UIItemTerm(m_itemScrollRect.content.GetChild(i).gameObject, this));
        }

        m_itemScrollView = new UIScrollView<ItemDB, UIItemTerm>(m_itemScrollRect, m_itemList, m_itemTermList);

        EventCenter.AddListener(EventType.UPDATE_ITEM_INFO, _UpdateInfoBoard);
        EventCenter.AddListener(EventType.UPDATE_ITEM_INFO, _UpdateScrollView);
      }

      public override void Destroy() {
        foreach (var itemTerm in m_itemTermList) {
          itemTerm.Destroy();
        }

        EventCenter.RemoveListener(EventType.UPDATE_ITEM_INFO, _UpdateInfoBoard);
        EventCenter.RemoveListener(EventType.UPDATE_ITEM_INFO, _UpdateScrollView);
      }

      private void _UpdateScrollView() {
        m_itemScrollView.UpdateView();
      }

      private async void _UpdateInfoBoard() {
        if (m_selectedItem != null) {
          if (m_selectedItem.num > 0) {
            var info = DBManager.instance.GetItemInfo(m_selectedItem.id);
            m_nameNum.text = info.name + "*" + m_selectedItem.num;
            m_description.text = info.description;
            m_iconImg.sprite = await ResourceManager.LoadAssetAsync<Sprite>(info.iconFileName);
          } else {
            m_selectedItem = null;
            m_nameNum.text = StringMap.EMPTY;
            m_description.text = StringMap.EMPTY;
            m_iconImg.sprite = m_noItemSprite;
          }
        }
      }

      public void SwitchMail(ItemDB m_item) {
        if(m_selectedItem != m_item) {
          m_selectedItem = m_item;
          _UpdateInfoBoard();

          m_itemScrollView.UpdateView(false);
        }
      }

      public override void OnUpdate() {
        m_itemScrollView.OnUpdate();
      }
    }

    private UIItemGroup m_itemGroup;

    public override void OnInit(UIData uiData, GameObject root) {
      base.OnInit(uiData, root);

      m_itemGroup = new UIItemGroup(GetGameObject("ItemGroup"));

      _AddListener();
    }

    public override void Close(Action callback = null, params object[] args) {
      DBManager.instance.UpdateUserData();
      
      base.Close(callback, args);
    }

    public override void OnUpdate() {
      m_itemGroup.OnUpdate();
    }

    private void _AddListener() {
      GetComponent<Button>("close").onClick.AddListener(() => UIManager.instance.CloseOne(name));
    }

    public override void Destroy() {
      base.Destroy();
      m_itemGroup.Destroy();
    }
  }
}