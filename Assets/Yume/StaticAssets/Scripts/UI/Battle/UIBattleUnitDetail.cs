using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Yume.Battle;
using Object = UnityEngine.Object;

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
            m_iconImg.sprite = ResourceManager.Load<Sprite>("noSprite");
          } else {
            m_iconImg.SetSpriteAsync(m_buff.icon);
          }
          string description = StringUtil.GetActualDescription(m_buff.description, m_buff.blackboard);
          if (m_buff.duration > 0) {
            description += $" 剩余回合数:{m_buff.duration - m_buff.turn}";
          }
          m_description.text = description;
        }
      }

      public IScrollTerm<Buff> Clone() {
        var clone = Object.Instantiate(gameObject, gameObject.GetParent());
        return new UIBuffTerm(clone);
      }
    }
    
    private class UIBuffGroup : UIChildBase, IScrollGroup<Buff,UIBuffTerm> {
      public Unit unit;
      private ScrollRect m_buffScrollRect;
      public List<Buff> dataList => m_buffList;
      private List<Buff> m_buffList = new List<Buff>();
      public List<UIBuffTerm> viewList => m_buffTermList;
      private UIScrollView<Buff, UIBuffTerm> m_buffScrollView;
      private List<UIBuffTerm> m_buffTermList = new List<UIBuffTerm>();

      public UIBuffGroup(GameObject root) : base(root) {
        m_buffScrollRect = GetComponent<ScrollRect>("buffScrollRect");

        for (int i = 0; i < m_buffScrollRect.content.childCount; i++) {
          m_buffTermList.Add(new UIBuffTerm(m_buffScrollRect.content.GetChild(i).gameObject));
        }

        m_buffScrollView = new UIScrollView<Buff, UIBuffTerm>(m_buffScrollRect, this);
      }
      
      public override void OnOpen() {
        m_buffList.Clear();
        foreach (var buffs in BattleManager.buffManagers[unit].buffDict.Values) {
          m_buffList.AddRange(buffs);
        }
        m_buffScrollView.ResetNormalizedPosition();
      }
      
      public override void OnUpdate() {
        m_buffScrollView.OnUpdate();
      }
    }

    private Text m_name;
    private Text[] m_attribs;
    private UIBuffGroup m_buffGroup;
    private UIGroupSwitch m_groupSwitch;

    public override void OnInit(UITemplate uiTemplate, GameObject gameObject) {
      base.OnInit(uiTemplate, gameObject);

      m_name = GetComponent<Text>("name");
      m_groupSwitch = GetComponent<UIGroupSwitch>("groupSwitch");
      
      m_attribs = new Text[Constant.ATTRIB_LENGTH];
      for (int i = 0; i < m_attribs.Length; i++) {
        m_attribs[i] = GetComponent<Text>($"attrib/{(AttribType)i}");
      }

      m_buffGroup = new UIBuffGroup(GetGameObject("buffGroup"));
      
      _AddListener();
    }

    private void _AddListener() {
      GetComponent<Button>("close").onClick.AddListener(() => UIManager.CloseOne(name));
    }

    public override void Open(Action callback = null, params object[] args) {
      Unit unit = (Unit)args[0];
      m_name.text = unit.name;

      for (int i = 0; i < m_attribs.Length; i++) {
        if (m_attribs[i] != null) {
          m_attribs[i].text = $"{((AttribType)i).GetDescription()}:{unit.attribs[i].value}";
        }
      }

      m_buffGroup.unit = unit;
      m_buffGroup.OnOpen();
      m_groupSwitch.OnOpen();
      
      base.Open(callback, args);
    }
    
    public override void OnUpdate() {
      m_buffGroup.OnUpdate();
    }
  }
}