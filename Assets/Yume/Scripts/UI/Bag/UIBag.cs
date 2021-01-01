using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Yume.Battle;
using Yume.Broadcast;
using Yume.DB;
using Yume.RedPointSystem;
using Yume.Resource;
using Yume.String;

namespace Yume.UI {
  public class UIBag : UIBase {
    private class UIItemTerm : IScrollTerm<ItemDB> {
      private ItemDB m_item;
      private Text m_numText;
      private Image m_iconImg;
      private GameObject m_root;
      private Button m_clickButton;
      public ItemDB item => m_item;
      private GameObject m_selected;
      private UIRedPoint m_itemRedPoint;
      public bool isValid => m_item != null;
      public GameObject gameObject => m_root;
      private UIItemInfoBoard m_itemInfoBoard;

      public UIItemTerm(UIItemInfoBoard itemInfoBoard, GameObject root) {
        m_root = root;
        var uiObject = root.GetComponent<UIObject>();
        m_selected = uiObject.GetGameObject("Selected");
        m_numText = uiObject.GetComponent<Text>("NumText");
        m_iconImg = uiObject.GetComponent<Image>("IconImg");
        m_clickButton = uiObject.GetComponent<Button>("ClickButton");

        m_itemInfoBoard = itemInfoBoard;

        var itemRedPointGo = root.FindGo(StringMap.UI_RED_POINT);
        m_itemRedPoint = new UIRedPoint(RedPointTreeType.MAIN_ITEM, m_item?.id, MainItemRedPointTree.MAIN_ITEM, itemRedPointGo);

        _AddListener();
      }

      private void _AddListener() {
        m_clickButton.onClick.AddListener(() => {
          m_itemInfoBoard.SetSelecteItem(m_item);
          m_itemInfoBoard.itemScrollView.UpdateViewList();
          if (m_item != null && !m_item.isViewed) {
            m_item.isViewed = true;
            //这个得手动存档一下
            DBManager.instance.UpdateUserData();
            //更新RedPoint
            RedPointManager.instance.SetRedPointNum(RedPointTreeType.MAIN_ITEM, m_item.id, MainItemRedPointTree.MAIN_ITEM, 0);
          }
        });
      }

      public async void Update(ItemDB data) {
        m_item = data;
        if (m_item == null) {
          gameObject.SetActiveEx(false);
        } else {
          gameObject.SetActiveEx(true);
          m_selected.SetActiveEx(m_item == m_itemInfoBoard.selectedItem);
          m_item = item;
          m_numText.text = StringUtil.IntegerToString(item.num);
          m_iconImg.gameObject.SetActiveEx(true);
          m_iconImg.sprite = await ResourceManager.LoadAssetAsync<Sprite>(
                                   DBManager.instance.GetItemInfo(item.id).iconFileName);
        }
        m_itemRedPoint.Update(m_item?.id);
      }

      public void Destroy() {
        m_itemRedPoint.Destroy();
      }
    }

    private class UIItemGroup {
      private List<ItemDB> m_itemList;
      private GridLayoutGroup m_itemGLG;
      private ScrollRect m_itemScrollRect;
      private UIItemInfoBoard m_itemInfoBoard;
      private UIScrollView<ItemDB, UIItemTerm> m_itemScrollView;
      public UIScrollView<ItemDB, UIItemTerm> itemScrollView => m_itemScrollView;
      private List<UIItemTerm> m_itemTermList = new List<UIItemTerm>();

      public UIItemGroup(GameObject root) {
        var uiObject = root.GetComponent<UIObject>();
        m_itemScrollRect = uiObject.GetComponent<ScrollRect>("ItemScrollRect");
        m_itemGLG = m_itemScrollRect.content.GetComponent<GridLayoutGroup>();
        m_itemInfoBoard = new UIItemInfoBoard(this, uiObject.GetGameObject("ItemInfoBoard"));
        m_itemList = DBManager.instance.userItems;

        for (int i = 0; i < m_itemScrollRect.content.childCount; i++) {
          m_itemTermList.Add(new UIItemTerm(m_itemInfoBoard, m_itemScrollRect.content.GetChild(i).gameObject));
        }

        m_itemScrollView = new UIScrollView<ItemDB, UIItemTerm>(m_itemScrollRect, m_itemGLG, m_itemList, m_itemTermList);

        EventCenter.AddListener(Broadcast.EventType.UPDATE_ITEM_INFO, m_itemScrollView.OnRefresh);
        EventCenter.AddListener(Broadcast.EventType.UPDATE_ITEM_INFO, m_itemInfoBoard.UpdateInfoBoard);
      }

      public void Destroy() {
        foreach (var itemTerm in m_itemTermList) {
          itemTerm.Destroy();
        }

        EventCenter.RemoveListener(Broadcast.EventType.UPDATE_ITEM_INFO, m_itemScrollView.OnRefresh);
        EventCenter.RemoveListener(Broadcast.EventType.UPDATE_ITEM_INFO, m_itemInfoBoard.UpdateInfoBoard);
      }
    }

    private class UIItemInfoBoard {
      private Text m_nameNum;
      private Text m_description;
      private Image m_iconImg;
      private Sprite m_defaultIcon;
      private ItemDB m_selectedItem;
      private UIItemGroup m_itemGroup;
      public ItemDB selectedItem => m_selectedItem;
      public UIScrollView<ItemDB, UIItemTerm> itemScrollView => m_itemGroup.itemScrollView;

      public UIItemInfoBoard(UIItemGroup itemGroup, GameObject root) {
        var uiObject = root.GetComponent<UIObject>();
        m_iconImg = uiObject.GetComponent<Image>("IconImage");
        m_nameNum = uiObject.GetComponent<Text>("NameNumText");
        m_description = uiObject.GetComponent<Text>("DescriptionText");

        m_itemGroup = itemGroup;

        m_defaultIcon = m_iconImg.sprite;
      }

      public async void UpdateInfoBoard() {
        if (m_selectedItem != null && m_selectedItem.num > 0) {
          var info = DBManager.instance.GetItemInfo(m_selectedItem.id);
          m_nameNum.text = info.name + "*" + m_selectedItem.num;
          m_description.text = info.description;
          m_iconImg.sprite = await ResourceManager.LoadAssetAsync<Sprite>(info.iconFileName);
        } else {
          m_selectedItem = null;
          m_nameNum.text = StringMap.EMPTY;
          m_description.text = StringMap.EMPTY;
          m_iconImg.sprite = m_defaultIcon;
        }
      }

      public void SetSelecteItem(ItemDB item) {
        m_selectedItem = item;
        UpdateInfoBoard();
      }
    }

    private UIItemGroup m_itemGroup;

    public override void OnInit(UIData uiData, GameObject root) {
      base.OnInit(uiData, root);

      m_itemGroup = new UIItemGroup(GetGameObject("ItemGroup"));

      _AddListener();
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