using System;
using UnityEngine;
using UnityEngine.UI;
using Yume.Battle;
using Yume.Data;
using Yume.Resource;

namespace Yume.UI {
  public class UICharDetail : UIBase {
    private Text m_lv;
    private Image m_illustration;
    private Text m_passiveSkillName;
    private Text m_passiveSkillDesc;
    private Image m_passiveSkillIcon;
    private Text[] m_attribs = new Text[Constant.ATTRIB_LENGTH];

    public override void OnInit(UITemplate uiTemplate, GameObject root) {
      base.OnInit(uiTemplate, root);

      m_lv = GetComponent<Text>("lv");
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
      PassiveSkillPhase passiveSkillPhase = unitTemplate.passiveSkillPhases[unit.passiveSkillLevels];
      Attrib[] attribs = MathUtil.CalculateAttribs(unitTemplate.id, unit.level, unitTemplate.maxLevel,
                                                   unitTemplate.GetMinAttribs(), unitTemplate.GetMaxAttribs());

      m_lv.text = $"Lv:{unit.level}/{unitTemplate.maxLevel}";
      m_illustration.sprite = ResourceManager.Load<Sprite>(unitTemplate.illustrationFileName);
      m_passiveSkillIcon.sprite = ResourceManager.Load<Sprite>(passiveSkillPhase.iconFileName);
      m_passiveSkillName.text = passiveSkillPhase.name;
      m_passiveSkillDesc.text = StringUtil.GetActualDescription(passiveSkillPhase.description, passiveSkillPhase.blackboard);
      for (int i = 0; i < m_attribs.Length; i++) {
        if (m_attribs[i] != null) {
          m_attribs[i].text = $"{((AttribType)i).GetDescription()}:{attribs[i].value}";
        }
      }

      base.Open(callback, args);      
    }
  }
}