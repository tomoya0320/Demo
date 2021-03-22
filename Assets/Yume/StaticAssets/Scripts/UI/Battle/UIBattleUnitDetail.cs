using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Yume.Battle;

namespace Yume.UI {
  public class UIBattleUnitDetail : UIBase {
    private class UIBuffTerm : UIChildBase, IScrollTerm<Buff> {
      public bool isValid { get; }
      
      public UIBuffTerm(GameObject root) : base(root) { }

      public override void OnOpen() {
        
      }

      public IScrollTerm<Buff> Clone() {
        throw new NotImplementedException();
      }

      public void UpdateScrollTerm(Buff data) {
        throw new NotImplementedException();
      }
    }
    
    private class UIBuffGroup : UIChildBase, IScrollGroup<Buff,UIBuffTerm> {
      public Unit unit;
      private List<Buff> m_buffList;
      private ScrollRect m_buffScrollRect;
      public List<Buff> dataList => m_buffList;
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
        m_buffScrollView.ResetNormalizedPosition();
      }
      
      public override void OnUpdate() {
        m_buffScrollView.OnUpdate();
      }
    }
    
    private Text m_lvText;
    private Text[] m_attribs;
    private Text m_passiveSkillName;
    private Text m_passiveSkillDesc;
    private Image m_passiveSkillIcon;
    private UIBuffGroup m_buffGroup;

    public override void OnInit(UITemplate uiTemplate, GameObject gameObject) {
      base.OnInit(uiTemplate, gameObject);

      m_lvText = GetComponent<Text>("lv");
      m_passiveSkillName = GetComponent<Text>("passiveSkillName");
      m_passiveSkillDesc = GetComponent<Text>("passiveSkillDesc");
      m_passiveSkillIcon = GetComponent<Image>("passiveSkillIcon");
      
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
      UnitTemplate unitTemplate = ResourceManager.Load<UnitTemplate>(unit.id);
      Attrib[] attribs = MathUtil.CalculateAttribs(unitTemplate.id, unit.level);

      m_lvText.text = $"Lv:{unit.level + 1}";

      for (int i = 0; i < m_attribs.Length; i++) {
        if (m_attribs[i] != null) {
          m_attribs[i].text = $"{((AttribType)i).GetDescription()}:{attribs[i].value}";
        }
      }

      PassiveSkillPhase passiveSkillPhase = unitTemplate.passiveSkillPhases[unit.passiveSkillLevel];
      m_passiveSkillName.text = $"Lv{unit.passiveSkillLevel + 1} {passiveSkillPhase.name}";
      m_passiveSkillIcon.SetSpriteAsync(passiveSkillPhase.iconFileName);
      m_passiveSkillDesc.text = StringUtil.GetActualDescription(passiveSkillPhase.description, passiveSkillPhase.buffTemplates);

      m_buffGroup.unit = unit;
      m_buffGroup.OnOpen();
      
      base.Open(callback, args);
    }
    
    public override void OnUpdate() {
      m_buffGroup.OnUpdate();
    }
  }
}