using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Yume.Battle;
using Yume.DB;
using Yume.Resource;

namespace Yume.UI {
  public class UIChar : UIBase {
    private class UICharTerm : UIChildBase, IScrollTerm<UnitDB> {
      private UnitDB m_char;
      private Image m_portrait;
      public bool isValid => m_char != null;

      public UICharTerm(GameObject root) : base(root) {
        m_portrait = GetComponent<Image>("Portrait");
 
        _AddListener();
      }

      private void _AddListener() {
        GetComponent<Button>("DetailButton").onClick.AddListener(() => UIManager.instance.OpenOne("char_detail", null, m_char));
      }

      public async void Update(UnitDB data) {
        m_char = data;

        gameObject.SetActiveEx(m_char != null);

        if (m_char != null) {
          UnitInfo unitInfo = DBManager.instance.GetUnitInfo(m_char.id);

          m_portrait.sprite = await ResourceManager.LoadAssetAsync<Sprite>(unitInfo.portraitFileName);
        }
      }
    }

    private class UICharGroup : UIChildBase {
      private List<UnitDB> m_charList;
      private GridLayoutGroup m_charGLG;
      private ScrollRect m_charScrollRect;
      private UIScrollView<UnitDB, UICharTerm> m_charScrollView;
      private List<UICharTerm> m_charTermList = new List<UICharTerm>();

      public UICharGroup(GameObject root) : base(root) {
        m_charScrollRect = GetComponent<ScrollRect>("CharScrollRect");
        m_charGLG = m_charScrollRect.content.GetComponent<GridLayoutGroup>();

        m_charList = DBManager.instance.userChars;

        for (int i = 0; i < m_charScrollRect.content.childCount; i++) {
          m_charTermList.Add(new UICharTerm(m_charScrollRect.content.GetChild(i).gameObject));
        }

        m_charScrollView = new UIScrollView<UnitDB, UICharTerm>(m_charScrollRect, m_charGLG, m_charList, m_charTermList);
      }
    }

    private UICharGroup m_charGroup;

    public override void OnInit(UIData uiData, GameObject root) {
      base.OnInit(uiData, root);

      m_charGroup = new UICharGroup(GetGameObject("CharGroup"));

      _AddListener();
    }

    private void _AddListener() {
      GetComponent<Button>("close").onClick.AddListener(() => UIManager.instance.CloseOne(name));
    }
  }
}