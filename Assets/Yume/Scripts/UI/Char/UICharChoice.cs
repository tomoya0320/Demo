using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Yume.Data;
using Yume.Prototype;
using Yume.Resource;

namespace Yume.UI {
  public class UICharChoice : UIBase {
    private class UICharChoiceTerm : UIChildBase, IScrollTerm<UnitData> {
      private UnitData m_character;
      private Button m_selectButton;
      private GameObject m_selected;
      public bool isValid => m_character != null;
      private UICharChoiceGroup m_charChoiceGroup;

      public UICharChoiceTerm(GameObject root, UICharChoiceGroup charChoiceGroup) : base(root) {
        m_selected = GetGameObject("Selected");
        m_selectButton = GetComponent<Button>("SelectButton");

        m_charChoiceGroup = charChoiceGroup;

        _AddListener();
      }

      private void _AddListener() {
        m_selectButton.onClick.AddListener(() => {
          if(m_charChoiceGroup.selectedChar == m_character) {
            m_charChoiceGroup.selectedChar = null;
          } else {
            m_charChoiceGroup.selectedChar = m_character;
          }
          m_charChoiceGroup.UpdateView(false);
        });
      }

      public async void UpdateScrollTerm(UnitData character) {
        m_character = character;

        gameObject.SetActiveEx(m_character != null);
        m_selected.SetActiveEx(m_charChoiceGroup.selectedChar == m_character);

        if (m_character != null) {
          UnitPrototype charPrototype = DataManager.instance.GetUnitPrototype(m_character.id);

          m_selectButton.image.sprite = await ResourceManager.LoadAssetAsync<Sprite>(charPrototype.portraitFileName);
        }
      }
    }

    private class UICharChoiceGroup : UIChildBase {
      public UnitData selectedChar;
      private List<UnitData> m_charChoiceList;
      private ScrollRect m_charChoiceScrollRect;
      private UIScrollView<UnitData, UICharChoiceTerm> m_charChoiceScrollView;
      private List<UICharChoiceTerm> m_charChoiceTermList = new List<UICharChoiceTerm>();

      public UICharChoiceGroup(GameObject root) : base(root) {
        m_charChoiceScrollRect = GetComponent<ScrollRect>("CharChoiceScrollRect");

        m_charChoiceList = DataManager.instance.userChars;

        for (int i = 0; i < m_charChoiceScrollRect.content.childCount; i++) {
          m_charChoiceTermList.Add(new UICharChoiceTerm(m_charChoiceScrollRect.content.GetChild(i).gameObject, this));
        }

        m_charChoiceScrollView = new UIScrollView<UnitData, UICharChoiceTerm>(m_charChoiceScrollRect, m_charChoiceList, m_charChoiceTermList);
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

    public override void OnInit(UIData uiData, GameObject root) {
      base.OnInit(uiData, root);

      m_charChoiceGroup = new UICharChoiceGroup(GetGameObject("CharChoiceGroup"));

      _AddListener();
    }

    public override void Open(Action callback = null, params object[] args) {
      string charId = (string)args[0];
      m_charChoiceGroup.selectedChar = DataManager.instance.GetChar(charId);
      m_onSelected = args[1] as Action<string>;

      m_charChoiceGroup.UpdateView(true);

      base.Open(callback, args);
    }

    public override void OnUpdate() {
      m_charChoiceGroup.OnUpdate();
    }

    private void _AddListener() {
      GetComponent<Button>("ok").onClick.AddListener(() => {
        UIManager.instance.CloseOne(name);
        m_onSelected?.Invoke(m_charChoiceGroup.selectedChar?.id);
      });
      GetComponent<Button>("cancel").onClick.AddListener(() => UIManager.instance.CloseOne(name));
    }
  }
}