using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Yume.Prototype;
using Yume.Data;
using Yume.Resource;

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
        m_detailButton.onClick.AddListener(() => UIManager.instance.OpenOne("char_detail", null, m_character));
      }

      public async void UpdateScrollTerm(UnitData character) {
        m_character = character;

        gameObject.SetActiveEx(m_character != null);

        if (m_character != null) {
          UnitPrototype charPrototype = DataManager.instance.GetUnitPrototype(m_character.id);

          m_detailButton.image.sprite = await ResourceManager.LoadAssetAsync<Sprite>(charPrototype.portraitFileName);
        }
      }
    }

    private class UICharGroup : UIChildBase {
      private List<UnitData> m_charList;
      private ScrollRect m_charScrollRect;
      private UIScrollView<UnitData, UICharTerm> m_charScrollView;
      private List<UICharTerm> m_charTermList = new List<UICharTerm>();

      public UICharGroup(GameObject root) : base(root) {
        m_charScrollRect = GetComponent<ScrollRect>("CharScrollRect");

        m_charList = DataManager.instance.userChars;

        for (int i = 0; i < m_charScrollRect.content.childCount; i++) {
          m_charTermList.Add(new UICharTerm(m_charScrollRect.content.GetChild(i).gameObject));
        }

        m_charScrollView = new UIScrollView<UnitData, UICharTerm>(m_charScrollRect, m_charList, m_charTermList);
      }

      public override void OnUpdate() {
        m_charScrollView.OnUpdate();
      }
    }

    private UICharGroup m_charGroup;

    public override void OnInit(UIData uiData, GameObject root) {
      base.OnInit(uiData, root);

      m_charGroup = new UICharGroup(GetGameObject("CharGroup"));

      _AddListener();
    }

    public override void OnUpdate() {
      m_charGroup.OnUpdate();
    }

    private void _AddListener() {
      GetComponent<Button>("close").onClick.AddListener(() => UIManager.instance.CloseOne(name));
    }
  }
}