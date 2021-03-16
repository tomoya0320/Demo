using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Yume.UI {
  public class UIChar : UIBase {
    private class UICharTerm : UIChildBase, IScrollTerm<UnitData> {
      private UnitData m_character;
      private Button m_detailButton;
      public bool isValid => m_character != null;

      public UICharTerm(GameObject root) : base(root) {
        m_detailButton = GetComponent<Button>("DetailButton");
 
        _AddListener();
      }

      private void _AddListener() {
        m_detailButton.onClick.AddListener(() => UIManager.OpenOne("char_detail", null, m_character));
      }

      public void UpdateScrollTerm(UnitData character) {
        m_character = character;

        gameObject.SetActiveEx(m_character != null);

        if (m_character != null) {
          UnitTemplate charTemplate = ResourceManager.Load<UnitTemplate>(m_character.id);

          m_detailButton.image.sprite = ResourceManager.Load<Sprite>(charTemplate.portraitFileName);
        }
      }

      public IScrollTerm<UnitData> Clone() {
        var clone = Object.Instantiate(gameObject, gameObject.GetParent());
        return new UICharTerm(clone);
      }
    }

    private class UICharGroup : UIChildBase, IScrollGroup<UnitData, UICharTerm> {
      private List<UnitData> m_charList;
      private ScrollRect m_charScrollRect;
      public List<UnitData> dataList => m_charList;
      public List<UICharTerm> viewList => m_charTermList;
      private UIScrollView<UnitData, UICharTerm> m_charScrollView;
      private List<UICharTerm> m_charTermList = new List<UICharTerm>();

      public UICharGroup(GameObject root) : base(root) {
        m_charScrollRect = GetComponent<ScrollRect>("CharScrollRect");
        
        m_charList = DataManager.playerChars;
        
        for (int i = 0; i < m_charScrollRect.content.childCount; i++) {
          m_charTermList.Add(new UICharTerm(m_charScrollRect.content.GetChild(i).gameObject));
        }

        m_charScrollView = new UIScrollView<UnitData, UICharTerm>(m_charScrollRect, this);
      }

      public override void OnOpen() {
        m_charList.Sort((left, right) => string.CompareOrdinal(left.id, right.id));
        m_charScrollView.ResetNormalizedPosition();
        m_charScrollView.UpdateView(true);
      }

      public override void OnUpdate() {
        m_charScrollView.OnUpdate();
      }
    }

    private UICharGroup m_charGroup;

    public override void OnInit(UITemplate uiTemplate, GameObject gameObject) {
      base.OnInit(uiTemplate, gameObject);

      m_charGroup = new UICharGroup(GetGameObject("CharGroup"));

      _AddListener();
    }

    public override void Open(Action callback = null, params object[] args) {
      m_charGroup.OnOpen();

      base.Open(callback, args);
    }

    public override void OnUpdate() {
      m_charGroup.OnUpdate();
    }

    private void _AddListener() {
      GetComponent<Button>("close").onClick.AddListener(() => UIManager.CloseOne(name));
    }
  }
}