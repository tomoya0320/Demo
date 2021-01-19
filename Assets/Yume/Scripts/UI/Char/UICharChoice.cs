using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Yume.Battle;
using Yume.DB;
using Yume.Resource;

namespace Yume.UI {
  public class UICharChoice : UIBase {
    private class UICharChoiceTerm : UIChildBase, IScrollTerm<UnitDB> {
      private UnitDB m_char;
      private Button m_selectButton;
      private GameObject m_selected;
      public bool isValid => m_char != null;
      private UICharChoiceGroup m_charChoiceGroup;

      public UICharChoiceTerm(GameObject root, UICharChoiceGroup charChoiceGroup) : base(root) {
        m_selected = GetGameObject("Selected");
        m_selectButton = GetComponent<Button>("SelectButton");

        m_charChoiceGroup = charChoiceGroup;

        _AddListener();
      }

      private void _AddListener() {
        m_selectButton.onClick.AddListener(() => {
          if(m_charChoiceGroup.selectedChar == m_char) {
            m_charChoiceGroup.selectedChar = null;
          } else {
            m_charChoiceGroup.selectedChar = m_char;
          }
          m_charChoiceGroup.UpdateViewData();
        });
      }

      public async void Update(UnitDB data) {
        m_char = data;

        gameObject.SetActiveEx(m_char != null);
        m_selected.SetActiveEx(m_charChoiceGroup.selectedChar == m_char);

        if (m_char != null) {
          UnitInfo unitInfo = DBManager.instance.GetUnitInfo(m_char.id);

          m_selectButton.image.sprite = await ResourceManager.LoadAssetAsync<Sprite>(unitInfo.portraitFileName);
        }
      }
    }

    private class UICharChoiceGroup : UIChildBase {
      public UnitDB selectedChar;
      private List<UnitDB> m_charChoiceList;
      private ScrollRect m_charChoiceScrollRect;
      private UIScrollView<UnitDB, UICharChoiceTerm> m_charChoiceScrollView;
      private List<UICharChoiceTerm> m_charChoiceTermList = new List<UICharChoiceTerm>();

      public UICharChoiceGroup(GameObject root) : base(root) {
        m_charChoiceScrollRect = GetComponent<ScrollRect>("CharChoiceScrollRect");

        m_charChoiceList = DBManager.instance.userChars;

        for (int i = 0; i < m_charChoiceScrollRect.content.childCount; i++) {
          m_charChoiceTermList.Add(new UICharChoiceTerm(m_charChoiceScrollRect.content.GetChild(i).gameObject, this));
        }

        m_charChoiceScrollView = new UIScrollView<UnitDB, UICharChoiceTerm>(m_charChoiceScrollRect, m_charChoiceList, m_charChoiceTermList);
      }

      public void OnRefresh() {
        m_charChoiceScrollView.OnRefresh();
      }

      public void UpdateViewData() {
        m_charChoiceScrollView.UpdateViewData();
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
      m_charChoiceGroup.selectedChar = DBManager.instance.GetCharDB(charId);
      m_onSelected = args[1] as Action<string>;

      m_charChoiceGroup.OnRefresh();

      base.Open(callback, args);
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