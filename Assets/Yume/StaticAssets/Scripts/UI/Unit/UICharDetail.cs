using System;
using UnityEngine;
using UnityEngine.UI;
using Yume.Battle;
using Yume.Broadcast;
using EventType = Yume.Broadcast.EventType;

namespace Yume.UI {
  public class UICharDetail : UIBase {
    private UnitData m_character;
    private Text m_lvText;
    private Text[] m_attribs;
    private Text m_description;
    private Image m_illustration;
    private Text m_passiveSkillName;
    private Text m_passiveSkillDesc;
    private Image m_passiveSkillIcon;
    private UnitTemplate m_unitTemplate;
    private Button m_updatePassiveSkillBtn;
    private bool isMaxPassiveSkillLv => m_character.passiveSkillLv == m_unitTemplate.passiveSkillPhases.Length - 1;

    public override void OnInit(UITemplate uiTemplate, GameObject gameObject) {
      base.OnInit(uiTemplate, gameObject);

      m_lvText = GetComponent<Text>("lv");
      m_description = GetComponent<Text>("description");
      m_illustration = GetComponent<Image>("illustration");
      m_passiveSkillName = GetComponent<Text>("passiveSkillName");
      m_passiveSkillDesc = GetComponent<Text>("passiveSkillDesc");
      m_passiveSkillIcon = GetComponent<Image>("passiveSkillIcon");
      m_updatePassiveSkillBtn = GetComponent<Button>("updatePassiveSkill");
      
      m_attribs = new Text[Constant.ATTRIB_LENGTH];
      for (int i = 0; i < m_attribs.Length; i++) {
        m_attribs[i] = GetComponent<Text>($"attrib/{(AttribType)i}");
      }

      _AddListener();

      EventCenter.AddListener(EventType.UPDATE_CHAR_DETAIL, _RefreshDetail);
    }

    private void _AddListener() {
      GetComponent<Button>("close").onClick.AddListener(() => UIManager.CloseOne(name));
      GetComponent<Button>("addExp").onClick.AddListener(
        () => UIManager.OpenOne("char_add_exp", null, m_character)
      );
      m_updatePassiveSkillBtn.onClick.AddListener(
        () => UIManager.OpenOne("char_update_passive_skill", null, m_character)
      );
    }

    public override void Open(Action callback = null, params object[] args) {
      m_character = args[0] as UnitData;
      m_unitTemplate = ResourceManager.Load<UnitTemplate>(m_character.id);
      _RefreshDetail();

      base.Open(callback, args);
    }

    public override void Destroy() {
      base.Destroy();
      EventCenter.RemoveListener(EventType.UPDATE_CHAR_DETAIL, _RefreshDetail);
    }

    private void _RefreshDetail() {
      Attrib[] attribs = MathUtil.CalculateAttribs(m_character.id, m_character.lv);

      m_description.text = m_unitTemplate.description;
      m_lvText.text = $"Lv:{m_character.lv + 1}/{m_unitTemplate.maxLevel + 1}";
      m_illustration.SetSpriteAsync(m_unitTemplate.illustrationFileName);

      for (int i = 0; i < m_attribs.Length; i++) {
        if (m_attribs[i] != null) {
          m_attribs[i].text = $"{((AttribType)i).GetDescription()}:{attribs[i].value}";
        }
      }

      m_updatePassiveSkillBtn.gameObject.SetActiveEx(!isMaxPassiveSkillLv);
      PassiveSkillPhase passiveSkillPhase = m_unitTemplate.passiveSkillPhases[m_character.passiveSkillLv];
      m_passiveSkillName.text = $"Lv{m_character.passiveSkillLv + 1} {passiveSkillPhase.name}";
      if (isMaxPassiveSkillLv) {
        m_passiveSkillName.text += "[MAX]";
      }
      m_passiveSkillIcon.SetSpriteAsync(passiveSkillPhase.iconFileName);
      m_passiveSkillDesc.text = StringUtil.GetActualDescription(passiveSkillPhase.description, passiveSkillPhase.buffTemplates);
    }
  }
}