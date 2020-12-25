using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Yume.Battle;
using Yume.Broadcast;
using Yume.DB;
using Yume.Resource;
using Yume.String;

namespace Yume.UI {
  public class UIBag : UIBase {
    private class UIItemTerm {
      private ItemDB m_item;
      private Text m_numText;
      private Image m_iconImg;
      private GameObject m_root;
      public GameObject root => m_root;
      private Button m_clickButton;
      public ItemDB item => m_item;
      private UIItemInfoBoard m_itemInfoBoard;
      public bool isValid => m_item != null && m_item.num > 0;

      public UIItemTerm(ItemDB item, UIItemInfoBoard itemInfoBoard, GameObject root) {
        m_root = root;
        var uiObject = root.GetComponent<UIObject>();
        m_numText = uiObject.GetComponent<Text>("NumText");
        m_iconImg = uiObject.GetComponent<Image>("IconImg");
        m_clickButton = uiObject.GetComponent<Button>("ClickButton");

        m_itemInfoBoard = itemInfoBoard;

        _OnInitInternal(item);
        _AddListener();
      }

      private void _AddListener() {
        m_clickButton.onClick.AddListener(() => {
          m_itemInfoBoard.SetSelecteItem(this);
        });
      }

      private void _OnInitInternal(ItemDB item) {
        if (m_item != null) {
          UpdateItem(item);
        }
      }

      public async void UpdateItem(ItemDB item) {
        m_item = item;
        m_numText.text = StringUtil.IntegerToString(item.num); 
        m_iconImg.gameObject.SetActive(true);
          m_iconImg.sprite = await ResourceManager.LoadAssetAsync<Sprite>(
                               DBManager.instance.GetItemInfo(item.id).iconFileName);
      }
    }

    private class UIItemGroup {
      private List<ItemDB> m_itemList;
      private GameObject m_itemTermPrefab;
      private RectTransform m_itemTermContent;
      private UIItemInfoBoard m_itemInfoBoard;
      private List<UIItemTerm> m_itemTermList = new List<UIItemTerm>();

      public UIItemGroup(GameObject root) {
        var uiObject = root.GetComponent<UIObject>();
        m_itemTermPrefab = uiObject.GetGameObject("ItemTermPrefab");
        m_itemTermContent = uiObject.GetGameObject("ItemTermContent").GetRect();
        m_itemInfoBoard = new UIItemInfoBoard(uiObject.GetGameObject("ItemInfoBoard"));
        m_itemList = DBManager.instance.userItems;

        foreach (var item in m_itemList) {
          var tempGo = Object.Instantiate(m_itemTermPrefab, m_itemTermContent);
          m_itemTermList.Add(new UIItemTerm(item, m_itemInfoBoard, tempGo));
        }

        EventCenter.AddListener(Broadcast.EventType.UPDATE_ITEM_INFO, _UpdateItemNum);
        EventCenter.AddListener(Broadcast.EventType.UPDATE_ITEM_INFO, m_itemInfoBoard.UpdateInfoBoard);
      }

      private void _UpdateItemNum() {
        for (int i = m_itemTermList.Count - 1; i >= 0; i--) {
          if (m_itemTermList[i].isValid) {
            m_itemTermList[i].UpdateItem(m_itemList[i]);
          } else {
            Object.Destroy(m_itemTermList[i].root);
            m_itemTermList.RemoveAt(i);
          }
        }
      }

      public void Destroy() {
        EventCenter.RemoveListener(Broadcast.EventType.UPDATE_ITEM_INFO, _UpdateItemNum);
        EventCenter.RemoveListener(Broadcast.EventType.UPDATE_ITEM_INFO, m_itemInfoBoard.UpdateInfoBoard);
      }
    }

    private class UIItemInfoBoard {
      private Text m_nameNum;
      private Text m_description;
      private Image m_iconImg;
      private Sprite m_defaultIcon;
      private UIItemTerm m_selectedItem;

      public UIItemInfoBoard(GameObject root) {
        var uiObject = root.GetComponent<UIObject>();
        m_defaultIcon = uiObject.GetSprite("DefaultIcon");
        m_iconImg = uiObject.GetComponent<Image>("IconImage");
        m_nameNum = uiObject.GetComponent<Text>("NameNumText");
        m_description = uiObject.GetComponent<Text>("DescriptionText");
      }

      public async void UpdateInfoBoard() {
        if (m_selectedItem != null && m_selectedItem.isValid) {
          var info = DBManager.instance.GetItemInfo(m_selectedItem.item.id);
          m_nameNum.text = info.name + "*" + m_selectedItem.item.num;
          m_description.text = info.description;
          m_iconImg.sprite = await ResourceManager.LoadAssetAsync<Sprite>(info.iconFileName);
        } else {
          m_nameNum.text = StringMap.EMPTY;
          m_description.text = StringMap.EMPTY;
          m_iconImg.sprite = m_defaultIcon;
        }
      }

      public void SetSelecteItem(UIItemTerm itemTerm) {
        m_selectedItem = itemTerm;
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