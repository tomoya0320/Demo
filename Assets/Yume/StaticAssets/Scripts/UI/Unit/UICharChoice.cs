using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Yume.UI {
  public class UICharChoice : UIBase {
    private class UICharChoiceTerm : UIChildBase, IScrollTerm<UnitData> {
      private UnitData m_character;
      private GameObject m_selected;
      private YButton m_selectButton;
      public UnitData character => m_character;
      public bool isValid => m_character != null;
      private UICharChoiceContainer m_charChoiceContainer;

      public UICharChoiceTerm(GameObject root, UICharChoiceContainer charChoiceContainer) : base(root) {
        m_selected = GetGameObject("Selected");
        m_selectButton = GetComponent<YButton>("SelectButton");

        m_charChoiceContainer = charChoiceContainer;

        _AddListener();
      }

      private void _AddListener() {
        m_selectButton.onClick.AddListener(() => {
          m_charChoiceContainer.selectedChar = m_charChoiceContainer.selectedChar == m_character ? null : m_character;
          m_charChoiceContainer.UpdateView(false);
        });

        m_selectButton.onLongPress += () => {
          if (m_character != null) {
            UIManager.OpenOne("char_detail", null, m_character);
          }
        };
      }

      public void UpdateScrollTerm(UnitData character) {
        m_character = character;

        gameObject.SetActiveEx(m_character != null);
        m_selected.SetActiveEx(m_charChoiceContainer.selectedChar == m_character);

        if (m_character != null) {
          m_selectButton.image.SetSprite(ResourceManager.Load<UnitTemplate>(m_character.id).portraitFileName);
        }
      }

      public IScrollTerm<UnitData> Clone() {
        var clone = ObjectUtil.Instantiate(gameObject, gameObject.GetParent());
        return new UICharChoiceTerm(clone, m_charChoiceContainer);
      }
    }

    private class UICharChoiceContainer : UIChildBase, IScrollContainer<UnitData, UICharChoiceTerm> {
      public UnitData selectedChar;
      private List<UnitData> m_charChoiceList;
      private ScrollRect m_charChoiceScrollRect;
      public List<UnitData> dataList => m_charChoiceList;
      public List<UICharChoiceTerm> viewList => m_charChoiceTermList;
      private UIScrollView<UnitData, UICharChoiceTerm> m_charChoiceScrollView;
      private List<UICharChoiceTerm> m_charChoiceTermList = new List<UICharChoiceTerm>();
      public UIScrollView<UnitData, UICharChoiceTerm> charChoiceScrollView => m_charChoiceScrollView;

      public UICharChoiceContainer(GameObject root) : base(root) {
        m_charChoiceScrollRect = GetComponent<ScrollRect>("CharChoiceScrollRect");
        
        m_charChoiceList = DataManager.playerChars;
        
        for (int i = 0; i < m_charChoiceScrollRect.content.childCount; i++) {
          m_charChoiceTermList.Add(new UICharChoiceTerm(m_charChoiceScrollRect.content.GetChild(i).gameObject, this));
        }

        m_charChoiceScrollView = new UIScrollView<UnitData, UICharChoiceTerm>(m_charChoiceScrollRect, this);
      }

      public override void OnUpdate() {
        m_charChoiceScrollView.OnUpdate();
      }

      public void UpdateView(bool recalLen) {
        m_charChoiceScrollView.UpdateView(recalLen);
      }

      public override void OnOpen() {
        m_charChoiceList.Sort((left, right) => string.CompareOrdinal(left.id, right.id));
        m_charChoiceScrollView.ResetNormalizedPos();
      }
    }

    private Action<string> m_onSelected;
    private UICharChoiceContainer m_charChoiceContainer;

    public override void OnInit(UITemplate uiTemplate, GameObject gameObject) {
      base.OnInit(uiTemplate, gameObject);

      m_charChoiceContainer = new UICharChoiceContainer(GetGameObject("CharChoiceContainer"));

      _AddListener();
    }

    public override void Open(Action callback = null, params object[] args) {
      string charId = (string)args[0];
      m_charChoiceContainer.selectedChar = DataManager.GetCharacter(charId);
      m_onSelected = args[1] as Action<string>;
      
      m_charChoiceContainer.OnOpen();

      base.Open(callback, args);
    }

    public override RectTransform GetDynamicListObj(string list, string id) {
      if (m_charChoiceContainer.gameObject.name == GameObject.Find(list)?.name) {
        int index = m_charChoiceContainer.dataList.FindLastIndex(data => data.id == id);
        if (index >= 0) {
          m_charChoiceContainer.charChoiceScrollView.SetAsFirstDisplay(index);
          var view = m_charChoiceContainer.viewList.FindLast(v => v.character?.id == id);
          return view.gameObject.GetComponent<RectTransform>();
        }
      }
      return null;
    }

    public override void OnUpdate() {
      m_charChoiceContainer.OnUpdate();
    }

    private void _AddListener() {
      GetComponent<Button>("ok").onClick.AddListener(() => {
        UIManager.CloseOne(name);
        m_onSelected?.Invoke(m_charChoiceContainer.selectedChar?.id);
      });
      GetComponent<Button>("cancel").onClick.AddListener(() => UIManager.CloseOne(name));
    }
  }
}