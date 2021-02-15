using System;
using UnityEngine;
using UnityEngine.UI;
using Yume.Battle;
using Yume.Data;
using Yume.Resource;

namespace Yume.UI {
  public class UICharDetail : UIBase {
    private Text m_lv;
    private Text m_description;
    private Image m_illustration;
    private Text m_passiveSkillName;
    private Text m_passiveSkillDesc;
    private Image m_passiveSkillIcon;
    private Text[] m_attribs = new Text[Constant.ATTRIB_LENGTH];

    public override void OnInit(UITemplate uiTemplate, GameObject gameObject) {
      base.OnInit(uiTemplate, gameObject);

      m_lv = GetComponent<Text>("lv");
      m_description = GetComponent<Text>("description");
      m_illustration = GetComponent<Image>("illustration");
      m_passiveSkillName = GetComponent<Text>("passiveSkillName");
      m_passiveSkillDesc = GetComponent<Text>("passiveSkillDesc");
      m_passiveSkillIcon = GetComponent<Image>("passiveSkillIcon");
      for (int i = 0; i < m_attribs.Length; i++) {
        m_attribs[i] = GetComponent<Text>($"attrib/{(AttribType)i}");
      }

      _AddListener();
    }

    private void _AddListener() {
      GetComponent<Button>("close").onClick.AddListener(() => UIManager.CloseOne(name));
    }

    public override void Open(Action callback = null, params object[] args) {
      UnitData unit = args[0] as UnitData;
      UnitTemplate unitTemplate = ResourceManager.Load<UnitTemplate>(unit.id);
      Attrib[] attribs = MathUtil.CalculateAttribs(unitTemplate.id, unit.level);

      m_description.text = unitTemplate.description;
      m_lv.text = $"Lv:{unit.level}/{unitTemplate.maxLevel}";
      m_illustration.sprite = ResourceManager.Load<Sprite>(unitTemplate.illustrationFileName);
      
      for (int i = 0; i < m_attribs.Length; i++) {
        if (m_attribs[i] != null) {
          m_attribs[i].text = $"{((AttribType)i).GetDescription()}:{attribs[i].value:0}";
        }
      }

      if (unit.passiveSkillLevel >= 0 && unit.passiveSkillLevel < unitTemplate.passiveSkillPhases.Length) {
        PassiveSkillPhase passiveSkillPhase = unitTemplate.passiveSkillPhases[unit.passiveSkillLevel];
        m_passiveSkillName.text = passiveSkillPhase.name;
        m_passiveSkillIcon.sprite = ResourceManager.Load<Sprite>(passiveSkillPhase.iconFileName);
        m_passiveSkillDesc.text = passiveSkillPhase.description;
      } else {
        m_passiveSkillName.text = String.Empty;
        m_passiveSkillDesc.text = String.Empty;
        m_passiveSkillIcon.sprite = ResourceManager.Load<Sprite>("noSprite");
      }

      base.Open(callback, args);      
    }
  }
}