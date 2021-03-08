using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Yume.Data;
using Yume.Resource;
using Object = UnityEngine.Object;

namespace Yume.UI {
  public class UICharChoice : UIBase {
    private class UICharChoiceTerm : UIChildBase, IScrollTerm<CharData> {
      private CharData m_character;
      private YButton m_selectButton;
      private GameObject m_selected;
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

      public void UpdateScrollTerm(CharData character) {
        m_character = character;

        gameObject.SetActiveEx(m_character != null);
        m_selected.SetActiveEx(m_charChoiceGroup.selectedChar == m_character);

        if (m_character != null) {
          UnitTemplate charTemplate = ResourceManager.Load<UnitTemplate>(m_character.id);

          m_selectButton.image.sprite = ResourceManager.Load<Sprite>(charTemplate.portraitFileName);
        }
      }

      public IScrollTerm<CharData> Clone() {
        var clone = Object.Instantiate(gameObject, gameObject.GetParent());
        return new UICharChoiceTerm(clone, m_charChoiceGroup);
      }
    }

    private class UICharChoiceGroup : UIChildBase, IScrollGroup<CharData, UICharChoiceTerm> {
      public CharData selectedChar;
      private List<CharData> m_charChoiceList;
      private ScrollRect m_charChoiceScrollRect;
      public List<CharData> dataList => m_charChoiceList;
      public List<UICharChoiceTerm> viewList => m_charChoiceTermList;
      private UIScrollView<CharData, UICharChoiceTerm> m_charChoiceScrollView;
      private List<UICharChoiceTerm> m_charChoiceTermList = new List<UICharChoiceTerm>();

      public UICharChoiceGroup(GameObject root) : base(root) {
        m_charChoiceScrollRect = GetComponent<ScrollRect>("CharChoiceScrollRect");

        m_charChoiceList = DataManager.playerChars;

        for (int i = 0; i < m_charChoiceScrollRect.content.childCount; i++) {
          m_charChoiceTermList.Add(new UICharChoiceTerm(m_charChoiceScrollRect.content.GetChild(i).gameObject, this));
        }

        m_charChoiceScrollView = new UIScrollView<CharData, UICharChoiceTerm>(m_charChoiceScrollRect, this);
      }

      public override void OnUpdate() {
        m_charChoiceScrollView.OnUpdate();
      }

      public void UpdateView(bool recalLen) {
        m_charChoiceScrollView.UpdateView(recalLen);
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

      m_charChoiceGroup.UpdateView(true);

      base.Open(callback, args);
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