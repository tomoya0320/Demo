using System;
using UnityEngine;
using UnityEngine.UI;
using Yume.Battle;

namespace Yume.UI {
  public class UIEnemyDetail : UIBase {
    private Text m_lvText;
    private Text m_description;
    private Image m_illustration;
    private Text m_passiveSkillName;
    private Text m_passiveSkillDesc;
    private Image m_passiveSkillIcon;
    private Text[] m_attribs = new Text[Constant.ATTRIB_LENGTH];

    public override void OnInit(UITemplate uiTemplate, GameObject gameObject) {
      base.OnInit(uiTemplate, gameObject);

      m_lvText = GetComponent<Text>("lv");
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
      UnitArgs enemyArgs = (UnitArgs)args[0];
      UnitTemplate unitTemplate = ResourceManager.Load<UnitTemplate>(enemyArgs.id);
      Attrib[] attribs = MathUtil.CalculateAttribs(unitTemplate.id, enemyArgs.lv);

      m_lvText.text = $"Lv:{enemyArgs.lv + 1}";
      m_description.text = unitTemplate.description;
      m_illustration.SetSpriteAsync(unitTemplate.illustrationFileName);

      for (int i = 0; i < m_attribs.Length; i++) {
        if (m_attribs[i] != null) {
          m_attribs[i].text = $"{((AttribType)i).GetDescription()}:{attribs[i].value}";
        }
      }

      PassiveSkillPhase passiveSkillPhase = unitTemplate.passiveSkillPhases[enemyArgs.passiveSkillLv];
      m_passiveSkillName.text = $"Lv{enemyArgs.passiveSkillLv + 1} {passiveSkillPhase.name}";
      m_passiveSkillIcon.SetSpriteAsync(passiveSkillPhase.iconFileName);
      m_passiveSkillDesc.text = StringUtil.GetActualDescription(passiveSkillPhase.description, passiveSkillPhase.buffTemplates);

      base.Open(callback, args);
    }
  }
}