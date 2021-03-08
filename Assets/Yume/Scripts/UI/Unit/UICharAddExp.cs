using System;
using System.Collections.Generic;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.UI;
using Yume.Battle;
using Yume.Data;
using Yume.Resource;
using Object = UnityEngine.Object;

namespace Yume.UI {
  public class UICharAddExp : UIBase {
    private class UIItemTerm : UIChildBase, IScrollTerm<ItemData> {
      private Text m_numText;
      private Image m_iconImg;
      private ItemData m_item;
      private YButton m_reduceBtn;
      private UIItemGroup m_itemGroup;
      public bool isValid => m_item != null;

      public UIItemTerm(GameObject root, UIItemGroup itemGroup) : base(root) {
        m_numText = GetComponent<Text>("NumText");
        m_iconImg = GetComponent<Image>("IconImg");
        m_reduceBtn = GetComponent<YButton>("reduce");

        m_itemGroup = itemGroup;
        
        _AddListener();
      }

      private void _AddListener() {
        YButton clickButton = GetComponent<YButton>("ClickButton");
        clickButton.onClick.AddListener(() => _AddExpItem(1));
        clickButton.onLongPressRepeat += () => _AddExpItem(4);
        m_reduceBtn.onClick.AddListener(() => _ReduceExpItem(1));
        m_reduceBtn.onLongPressRepeat += () => _ReduceExpItem(4);
      }

      private void _AddExpItem(int deltaCount) {
        if (m_item == null || deltaCount < 0) {
          return;
        }

        if (m_itemGroup.charAddExp.isMaxLv) {
          return;
        }

        m_itemGroup.selectedExpDict.TryGetValue(m_item.id, out int count);
        if (count < m_item.count) {
          m_itemGroup.selectedExpDict[m_item.id] = Mathf.Min(count + deltaCount, m_item.count);
          // 刷新角色等级、经验、属性等信息
          m_itemGroup.charAddExp._RefreshDetail();
          // 刷新自己道具的信息
          UpdateScrollTerm(m_item);
        }
      }

      private void _ReduceExpItem(int deltaCount) {
        if (m_item == null || deltaCount < 0) {
          return;
        }
        m_itemGroup.selectedExpDict.TryGetValue(m_item.id, out int count);
        if (count > 0) {
          m_itemGroup.selectedExpDict[m_item.id] = Mathf.Max(count - deltaCount, 0);
          if (m_itemGroup.selectedExpDict[m_item.id] <= 0) {
            m_itemGroup.selectedExpDict.Remove(m_item.id);
          }
          // 刷新角色等级、经验、属性等信息
          m_itemGroup.charAddExp._RefreshDetail();
          // 刷新自己道具的信息
          UpdateScrollTerm(m_item);
        }
      }

      public void UpdateScrollTerm(ItemData item) {
        m_item = item;

        gameObject.SetActiveEx(m_item != null);
        if (m_item != null && m_itemGroup.selectedExpDict.TryGetValue(m_item.id, out int count)) {
          m_reduceBtn.gameObject.SetActiveEx(true);
          m_reduceBtn.textComponent.text = count.ToString();
        } else {
          m_reduceBtn.gameObject.SetActiveEx(false);
        }

        if (m_item != null) {
          m_numText.text = StringUtil.IntegerToString(m_item.count);
          m_iconImg.sprite = ResourceManager.Load<Sprite>(ResourceManager.Load<ItemTemplate>(m_item.id).iconFileName);
        }
      }

      public IScrollTerm<ItemData> Clone() {
        var clone = Object.Instantiate(gameObject, gameObject.GetParent());
        return new UIItemTerm(clone, m_itemGroup);
      }
    }

    private class UIItemGroup : UIChildBase, IScrollGroup<ItemData, UIItemTerm> {
      private List<ItemData> m_itemList;
      private ScrollRect m_itemScrollRect;
      public UICharAddExp charAddExp { get; }
      public List<ItemData> dataList => m_itemList;
      public List<UIItemTerm> viewList => m_itemTermList;
      private UIScrollView<ItemData, UIItemTerm> m_itemScrollView;
      private List<UIItemTerm> m_itemTermList = new List<UIItemTerm>();
      public Dictionary<string, int> selectedExpDict = new Dictionary<string, int>();
      private Predicate<ItemData> m_findDisplayItem = item => {
        var itemTemplate = ResourceManager.Load<ItemTemplate>(item.id);
        return !itemTemplate.hideDisplay && itemTemplate.groupId == StringMap.CHAR_EXP;
      };

      public UIItemGroup(GameObject root, UICharAddExp charAddExp) : base(root) {
        m_itemScrollRect = GetComponent<ScrollRect>("ItemScrollRect");

        this.charAddExp = charAddExp;
        
        m_itemList = DataManager.playerItems.FindAll(m_findDisplayItem);

        for (int i = 0; i < m_itemScrollRect.content.childCount; i++) {
          m_itemTermList.Add(new UIItemTerm(m_itemScrollRect.content.GetChild(i).gameObject, this));
        }

        m_itemScrollView = new UIScrollView<ItemData, UIItemTerm>(m_itemScrollRect, this);
      }

      public override void Destroy() {
        foreach (var itemTerm in m_itemTermList) {
          itemTerm.Destroy();
        }
      }

      public void RefreshScrollView() {
        m_itemList = DataManager.playerItems.FindAll(m_findDisplayItem);
        m_itemScrollView.UpdateView();
      }

      public override void OnUpdate() {
        m_itemScrollView.OnUpdate();
      }

      public override void OnOpen(params object[] args) {
        selectedExpDict.Clear();
        m_itemScrollView.UpdateView(true);
      }
    }

    private int m_lv;
    private int m_exp;
    private int m_consumeGold;
    private CharData m_character;
    private UnitTemplate m_unitTemplate;
    private UIItemGroup m_itemGroup;
    public bool isMaxLv => m_lv >= m_unitTemplate.maxLevel;
    
    private Text m_lvText;
    private YImage m_expImage; 
    private Text m_goldText;
    private Text m_consumeGoldText;
    private Text[] m_attribs = new Text[Constant.ATTRIB_LENGTH];

    public override void OnInit(UITemplate uiTemplate, GameObject gameObject) {
      base.OnInit(uiTemplate, gameObject);
      
      m_itemGroup = new UIItemGroup(GetGameObject("ItemGroup"), this);
      
      m_lvText = GetComponent<Text>("lv");
      m_goldText = GetComponent<Text>("gold");
      m_expImage = GetComponent<YImage>("exp");
      m_consumeGoldText = GetComponent<Text>("consume gold");
      for (int i = 0; i < m_attribs.Length; i++) {
        m_attribs[i] = GetComponent<Text>($"attrib/{(AttribType)i}");
      }
      
      _AddListener();
    }

    public override void Open(Action callback = null, params object[] args) {
      base.Open(callback, args);

      m_character = args[0] as CharData;
      m_unitTemplate = ResourceManager.Load<UnitTemplate>(m_character.id);
      m_itemGroup.OnOpen(args);

      _RefreshDetail();
    }

    private void _RefreshDetail() {
      int totalExp = _GetSelectedExp();
      DataManager.IfAddCharExp(
        m_character.id, m_character.lv, m_character.exp,
        totalExp, out m_lv, out m_exp
      );
      m_consumeGold = MathUtil.CalNeedGoldByCharExp(totalExp);
      m_lvText.text = $"Lv:{m_lv}/{m_unitTemplate.maxLevel}";
      m_goldText.text = DataManager.GetItemCount(StringMap.GOLD_ITEM_ID).ToString();
      m_consumeGoldText.text = string.Format(StringMap.CONSUME_GOLD, m_consumeGold);
      if (isMaxLv) {
        m_expImage.fillAmount = 1.0f;
        m_expImage.textComponent.text = "EXP:-/-";
      } else {
        float needExp = MathUtil.CalNeedCharExp(m_lv);
        m_expImage.fillAmount = m_exp / needExp;
        m_expImage.textComponent.text = $"EXP:{m_exp}/{needExp}"; 
      }
      Attrib[] oldAttribs = MathUtil.CalculateAttribs(m_character.id, m_character.lv);
      Attrib[] newAttribs = MathUtil.CalculateAttribs(m_character.id, m_lv);
      // TODO:测试用
      string color = m_character.lv < m_lv ? "#3232CD" : "#000000";
      for (int i = 0; i < m_attribs.Length; i++) {
        if (m_attribs[i] != null) {
          m_attribs[i].text = $"{((AttribType)i).GetDescription()}:{(int)oldAttribs[i].value}---><color={color}>{(int)newAttribs[i].value}</color>";
        }
      }
    }

    private int _GetSelectedExp() {
      int exp = 0;
      foreach (var kv in m_itemGroup.selectedExpDict) {
        Blackboard blackboard = ResourceManager.Load<ItemTemplate>(kv.Key).blackboard;
        exp += kv.Value * blackboard.GetValue<int>("exp");
      }
      return exp;
    }

    private void _AddListener() {
      GetComponent<Button>("close").onClick.AddListener(() => UIManager.CloseOne(name));
      GetComponent<Button>("ok").onClick.AddListener(
        () => {
          int totalExp = _GetSelectedExp();
          // 增加角色经验
          if (totalExp > 0) {
            if (DataManager.GetItemCount(StringMap.GOLD_ITEM_ID) < m_consumeGold) {
              MessageManager.AddMessage(new Message(Message.Style.FADE, "金币不足！", false));
              return;
            }
            // 消耗道具
            DataManager.ConsumeItem(StringMap.GOLD_ITEM_ID, m_consumeGold);
            m_itemGroup.selectedExpDict.ForEach(
              kv => {
                DataManager.ConsumeItem(kv.Key, kv.Value);
              }
            );
            // 增加经验
            DataManager.AddCharExp(m_character, totalExp);
            // 清除选中的经验值
            _ClearSelectedExp();
          }
        }
      );
      GetComponent<Button>("clear").onClick.AddListener(_ClearSelectedExp);
    }

    private void _ClearSelectedExp() {
      m_itemGroup.selectedExpDict.Clear();
      m_itemGroup.RefreshScrollView();
      _RefreshDetail();
    }

    public override void OnUpdate() {
      m_itemGroup.OnUpdate();
    }

    public override void Destroy() {
      base.Destroy();
      m_itemGroup.Destroy();
    }
  }
}