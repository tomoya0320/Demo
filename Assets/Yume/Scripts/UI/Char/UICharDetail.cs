using System;
using UnityEngine;
using UnityEngine.UI;
using Yume.Battle;
using Yume.DB;
using Yume.Resource;

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
      UnitDB unitDB = args[0] as UnitDB;
      UnitInfo unitInfo = DBManager.instance.GetUnitInfo(unitDB.id);
      PassiveSkillPhase passiveSkillPhase = unitInfo.passiveSkillPhases[unitDB.passiveSkillLevels];
      Attrib[] attribs = MathUtil.CalculateAttribs(unitInfo.id, unitDB.level, unitInfo.maxLevel,
                                                   unitInfo.GetMinAttribs(), unitInfo.GetMaxAttribs());

      m_lv.text = $"Lv:{unitDB.level}/{unitInfo.maxLevel}";
      m_illustration.sprite = await ResourceManager.LoadAssetAsync<Sprite>(unitInfo.illustrationFileName);
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