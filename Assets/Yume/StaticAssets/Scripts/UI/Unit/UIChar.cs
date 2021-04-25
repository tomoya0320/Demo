using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
          m_detailButton.image.SetSpriteAsync(ResourceManager.Load<UnitTemplate>(m_character.id).portraitFileName);
        }
      }

      public IScrollTerm<UnitData> Clone() {
        var clone = ObjectUtil.Instantiate(gameObject, gameObject.GetParent());
        return new UICharTerm(clone);
      }
    }

    private class UICharContainer : UIChildBase, IScrollContainer<UnitData, UICharTerm> {
      private List<UnitData> m_charList;
      private ScrollRect m_charScrollRect;
      public List<UnitData> dataList => m_charList;
      public List<UICharTerm> viewList => m_charTermList;
      private UIScrollView<UnitData, UICharTerm> m_charScrollView;
      private List<UICharTerm> m_charTermList = new List<UICharTerm>();

      public UICharContainer(GameObject root) : base(root) {
        m_charScrollRect = GetComponent<ScrollRect>("CharScrollRect");
        
        m_charList = DataManager.playerChars;
        
        for (int i = 0; i < m_charScrollRect.content.childCount; i++) {
          m_charTermList.Add(new UICharTerm(m_charScrollRect.content.GetChild(i).gameObject));
        }

        m_charScrollView = new UIScrollView<UnitData, UICharTerm>(m_charScrollRect, this);
      }

      public override void OnOpen() {
        m_charList.Sort((left, right) => string.CompareOrdinal(left.id, right.id));
        m_charScrollView.ResetNormalizedPos();
      }

      public override void OnUpdate() {
        m_charScrollView.OnUpdate();
      }
    }

    private UICharContainer m_charContainer;

    public override void OnInit(UITemplate uiTemplate, GameObject gameObject) {
      base.OnInit(uiTemplate, gameObject);

      m_charContainer = new UICharContainer(GetGameObject("CharContainer"));

      _AddListener();
    }

    public override void Open(Action callback = null, params object[] args) {
      m_charContainer.OnOpen();

      base.Open(callback, args);
    }

    public override void OnUpdate() {
      m_charContainer.OnUpdate();
    }

    private void _AddListener() {
      GetComponent<Button>("close").onClick.AddListener(() => UIManager.CloseOne(name));
    }
  }
}