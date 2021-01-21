using System;
using UnityEngine;
using UnityEngine.UI;
using Yume.Battle;
using Yume.Data;
using Yume.Resource;
using Yume.Prototype;

namespace Yume.UI {
  public class UICharDetail : UIBase {
    private Text m_lv;
    private Image m_illustration;
    private Text m_passiveSkillName;
    private Text m_passiveSkillDesc;
    private Image m_passiveSkillIcon;
    private Text[] m_attribs = new Text[(int)AttribType.LENGTH];

    public override void OnInit(UIData uiData, GameObject root) {
      base.OnInit(uiData, root);

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
      GetComponent<Button>("close").onClick.AddListener(() => UIManager.instance.CloseOne(name));
    }

    public async override void Open(Action callback = null, params object[] args) {
      UnitData unit = args[0] as UnitData;
      UnitPrototype unitPrototype = DataManager.instance.GetUnitPrototype(unit.id);
      PassiveSkillPhase passiveSkillPhase = unitPrototype.passiveSkillPhases[unit.passiveSkillLevels];
      Attrib[] attribs = MathUtil.CalculateAttribs(unitPrototype.id, unit.level, unitPrototype.maxLevel,
                                                   unitPrototype.GetMinAttribs(), unitPrototype.GetMaxAttribs());

      m_lv.text = $"Lv:{unit.level}/{unitPrototype.maxLevel}";
      m_illustration.sprite = await ResourceManager.LoadAssetAsync<Sprite>(unitPrototype.illustrationFileName);
      m_passiveSkillIcon.sprite = await ResourceManager.LoadAssetAsync<Sprite>(passiveSkillPhase.iconFileName);
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