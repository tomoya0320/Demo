using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Yume.Battle;

namespace Yume.UI {
  public class UIBattleUnitDetail : UIBase {
    private class UIBuffTerm : UIChildBase, IScrollTerm<Buff> {
      private Buff m_buff;
      private Image m_iconImg;
      private Text m_description;
      public bool isValid => m_buff != null;

      public UIBuffTerm(GameObject root) : base(root) {
        m_iconImg = GetComponent<Image>("icon");
        m_description = GetComponent<Text>("description");
      }

      public void UpdateScrollTerm(Buff buff) {
        m_buff = buff;

        gameObject.SetActiveEx(m_buff != null);

        if (m_buff != null) {
          if (string.IsNullOrEmpty(m_buff.icon)) {
            m_iconImg.SetSprite(StringMap.NO_SPRITE);
          } else {
            m_iconImg.SetSprite(m_buff.icon);
          }
          string description = m_buff.description;
          if (m_buff.duration > 0) {
            description = description.Replace("{0}", $"{m_buff.duration - m_buff.turn}");
          }
          m_description.text = StringUtil.GetActualDescription(description, m_buff.blackboard);
        }
      }

      public IScrollTerm<Buff> Clone() {
        var clone = ObjectUtil.Instantiate(gameObject, gameObject.GetParent());
        return new UIBuffTerm(clone);
      }
    }

    private class UIBuffContainer : UIChildBase, IScrollContainer<Buff, UIBuffTerm> {
      public Unit unit;
      private ScrollRect m_buffScrollRect;
      public List<Buff> dataList => m_buffList;
      private List<Buff> m_buffList = new List<Buff>();
      public List<UIBuffTerm> viewList => m_buffTermList;
      private UIScrollView<Buff, UIBuffTerm> m_buffScrollView;
      private List<UIBuffTerm> m_buffTermList = new List<UIBuffTerm>();

      public UIBuffContainer(GameObject root) : base(root) {
        m_buffScrollRect = GetComponent<ScrollRect>("buffScrollRect");

        for (int i = 0; i < m_buffScrollRect.content.childCount; i++) {
          m_buffTermList.Add(new UIBuffTerm(m_buffScrollRect.content.GetChild(i).gameObject));
        }

        m_buffScrollView = new UIScrollView<Buff, UIBuffTerm>(m_buffScrollRect, this);
      }

      public override void OnOpen() {
        m_buffScrollView.ResetNormalizedPos();
      }

      public override void OnUpdate() {
        m_buffList.Clear();
        foreach (var buffs in BattleManager.buffManagers[unit].buffDict.Values) {
          m_buffList.AddRange(buffs);
        }
        
        m_buffScrollView.OnUpdate(true);
      }
    }

    private Text m_name;
    private Text[] m_attribs;
    private UIBuffContainer m_buffContainer;
    private UIGroupSwitch m_unitInfoGroup;

    public override void OnInit(UITemplate uiTemplate, GameObject gameObject) {
      base.OnInit(uiTemplate, gameObject);

      m_name = GetComponent<Text>("name");
      m_unitInfoGroup = GetComponent<UIGroupSwitch>("groupSwitch");

      m_attribs = new Text[Constant.ATTRIB_LENGTH];
      for (int i = 0; i < m_attribs.Length; i++) {
        m_attribs[i] = GetComponent<Text>($"attrib/{(AttribType)i}");
      }

      m_buffContainer = new UIBuffContainer(GetGameObject("buffContainer"));
      
      _AddListener();
    }

    private void _AddListener() {
      GetComponent<Button>("close").onClick.AddListener(() => UIManager.CloseOne(name));
    }

    public override void Open(Action callback = null, params object[] args) {
      m_buffContainer.unit = args[0] as Unit;
      m_name.text = m_buffContainer.unit.name;

      m_buffContainer.OnOpen();
      m_unitInfoGroup.OnOpen();

      base.Open(callback, args);
    }

    public override void OnUpdate() {
      m_buffContainer.OnUpdate();
      
      for (int i = 0; i < m_attribs.Length; i++) {
        if (m_attribs[i] != null) {
          m_attribs[i].text = $"{((AttribType)i).GetDescription()}:{m_buffContainer.unit.attribs[i].value}";
        }
      }
    }
  }
}