using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Yume.UI {
  public class UICharChoice : UIBase {
    private class UICharChoiceTerm : UIChildBase, IScrollTerm<UnitData> {
      private UnitData m_character;
      private GameObject m_selected;
      private YButton m_selectButton;
      public UnitData character => m_character;
      public bool isValid => m_character != null;
      private UICharChoiceGroup m_charChoiceGroup;

      public UICharChoiceTerm(GameObject root, UICharChoiceGroup charChoiceGroup) : base(root) {
        m_selected = GetGameObject("Selected");
        m_selectButton = GetComponent<YButton>("SelectButton");

        m_charChoiceGroup = charChoiceGroup;

        _AddListener();
      }

      private void _AddListener() {
        m_selectButton.onClick.AddListener(() => {
          m_charChoiceGroup.selectedChar = m_charChoiceGroup.selectedChar == m_character ? null : m_character;
          m_charChoiceGroup.UpdateView(false);
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
        m_selected.SetActiveEx(m_charChoiceGroup.selectedChar == m_character);

        if (m_character != null) {
          m_selectButton.image.SetSpriteAsync(ResourceManager.Load<UnitTemplate>(m_character.id).portraitFileName);
        }
      }

      public IScrollTerm<UnitData> Clone() {
        var clone = Object.Instantiate(gameObject, gameObject.GetParent());
        return new UICharChoiceTerm(clone, m_charChoiceGroup);
      }
    }

    private class UICharChoiceGroup : UIChildBase, IScrollGroup<UnitData, UICharChoiceTerm> {
      public UnitData selectedChar;
      private List<UnitData> m_charChoiceList;
      private ScrollRect m_charChoiceScrollRect;
      public List<UnitData> dataList => m_charChoiceList;
      public List<UICharChoiceTerm> viewList => m_charChoiceTermList;
      private UIScrollView<UnitData, UICharChoiceTerm> m_charChoiceScrollView;
      private List<UICharChoiceTerm> m_charChoiceTermList = new List<UICharChoiceTerm>();
      public UIScrollView<UnitData, UICharChoiceTerm> charChoiceScrollView => m_charChoiceScrollView;

      public UICharChoiceGroup(GameObject root) : base(root) {
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
        m_charChoiceScrollView.ResetNormalizedPosition();
        UpdateView(true);
      }
    }

    private Action<string> m_onSelected;
    private UICharChoiceGroup m_charChoiceGroup;

    public override void OnInit(UITemplate uiTemplate, GameObject gameObject) {
      base.OnInit(uiTemplate, gameObject);

      m_charChoiceGroup = new UICharChoiceGroup(GetGameObject("CharChoiceGroup"));

      _AddListener();
    }

    public override void Open(Action callback = null, params object[] args) {
      string charId = (string)args[0];
      m_charChoiceGroup.selectedChar = DataManager.GetCharacter(charId);
      m_onSelected = args[1] as Action<string>;
      
      m_charChoiceGroup.OnOpen();

      base.Open(callback, args);
    }

    public override RectTransform GetDynamicListObj(string list, string id) {
      if (m_charChoiceGroup.gameObject.name == list) {
        int index = m_charChoiceGroup.dataList.FindLastIndex(data => data.id == id);
        if (index >= 0) {
          m_charChoiceGroup.charChoiceScrollView.SetAsFirstDisplay(index);
          var view = m_charChoiceGroup.viewList.FindLast(v => v.character?.id == id);
          return view.gameObject.GetComponent<RectTransform>();
        }
      }
      return null;
    }

    public override void OnUpdate() {
      m_charChoiceGroup.OnUpdate();
    }

    private void _AddListener() {
      GetComponent<Button>("ok").onClick.AddListener(() => {
        UIManager.CloseOne(name);
        m_onSelected?.Invoke(m_charChoiceGroup.selectedChar?.id);
      });
      GetComponent<Button>("cancel").onClick.AddListener(() => UIManager.CloseOne(name));
    }
  }
}