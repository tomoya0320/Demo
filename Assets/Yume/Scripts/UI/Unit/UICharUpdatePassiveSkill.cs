using System;
using UnityEngine;
using UnityEngine.UI;
using Yume.Data;
using Yume.Resource;

namespace Yume.UI {
  public class UICharUpdatePassiveSkill : UIGaussianBlurBg {
    private Text m_oldPassiveSkillName;
    private Text m_oldPassiveSkillDesc;
    private Image m_oldPassiveSkillIcon;  
    
    private Text m_newPassiveSkillName;
    private Text m_newPassiveSkillDesc;
    private Image m_newPassiveSkillIcon;

    private Button m_updateBtn;
    private UnitData m_character;
    private UnitTemplate m_unitTemplate;
    private Transform m_requestItems;
    private PassiveSkillPhase passiveSkillPhase => m_unitTemplate.passiveSkillPhases[m_character.passiveSkillLv];
    private PassiveSkillPhase nextPassiveSkillPhase => isMaxPassiveSkillLv ? passiveSkillPhase : m_unitTemplate.passiveSkillPhases[m_character.passiveSkillLv + 1];
    private bool isMaxPassiveSkillLv => m_character.passiveSkillLv == m_unitTemplate.passiveSkillPhases.Length - 1;
    
    public override void OnInit(UITemplate uiTemplate, GameObject gameObject) {
      base.OnInit(uiTemplate, gameObject);

      m_updateBtn = GetComponent<Button>("ok");
      m_requestItems = GetTransform("request items");

      m_oldPassiveSkillName = GetComponent<Text>("old name");
      m_oldPassiveSkillDesc = GetComponent<Text>("old desc");
      m_oldPassiveSkillIcon = GetComponent<Image>("old icon");  
      
      m_newPassiveSkillName = GetComponent<Text>("new name");
      m_newPassiveSkillDesc = GetComponent<Text>("new desc");
      m_newPassiveSkillIcon = GetComponent<Image>("new icon");
      
      _AddListener();
    }

    public override void Open(Action callback = null, params object[] args) {
      m_character = args[0] as UnitData;
      m_unitTemplate = ResourceManager.Load<UnitTemplate>(m_character.id);
      
      _RefreshDetail();
      
      base.Open(callback, args);
    }

    private void _RefreshDetail() {
      if (isMaxPassiveSkillLv) {
        m_updateBtn.gameObject.SetActiveEx(false);
        for (int i = 0; i < m_requestItems.childCount; i++) {
          var tempGo = m_requestItems.GetChild(i).gameObject;
          tempGo.SetActiveEx(false);
        }
      } else {
        m_updateBtn.gameObject.SetActiveEx(true);
        for (int i = 0; i < m_requestItems.childCount; i++) {
          var tempGo = m_requestItems.GetChild(i).gameObject;
          if (i < passiveSkillPhase.updateRequestItems.Length) {
            ItemData itemData = passiveSkillPhase.updateRequestItems[i];
            tempGo.SetActiveEx(true);
            tempGo.GetComponentInChildren<Image>().sprite = ResourceManager.Load<Sprite>
              (ResourceManager.Load<ItemTemplate>(itemData.id).iconFileName);
            tempGo.GetComponentInChildren<Text>().text = itemData.count.ToString();
          } else {
            tempGo.SetActiveEx(false);
          }
        }
      }
      m_oldPassiveSkillName.text = $"Lv{m_character.passiveSkillLv + 1} {passiveSkillPhase.name}";
      m_oldPassiveSkillIcon.sprite = ResourceManager.Load<Sprite>(passiveSkillPhase.iconFileName);
      m_oldPassiveSkillDesc.text = StringUtil.GetActualDescription(passiveSkillPhase.description, passiveSkillPhase.buffTemplates);

      if (isMaxPassiveSkillLv) {
        m_newPassiveSkillName.text = "等级已达到最大值";
        m_newPassiveSkillIcon.sprite = ResourceManager.Load<Sprite>("noSprite");
        m_newPassiveSkillDesc.text = StringMap.EMPTY;
      } else {
        m_newPassiveSkillName.text = $"Lv{m_character.passiveSkillLv + 2} {nextPassiveSkillPhase.name}";
        m_newPassiveSkillIcon.sprite = ResourceManager.Load<Sprite>(nextPassiveSkillPhase.iconFileName);
        m_newPassiveSkillDesc.text = StringUtil.GetActualDescription(nextPassiveSkillPhase.description, nextPassiveSkillPhase.buffTemplates);  
      }
    }

    private void _AddListener() {
      GetComponent<Button>("close").onClick.AddListener(() => UIManager.CloseOne(name));
      m_updateBtn.onClick.AddListener(
        () => {
          if (!isMaxPassiveSkillLv) {
            foreach (var item in passiveSkillPhase.updateRequestItems) {
              if (DataManager.GetItemCount(item.id) < item.count) {
                MessageManager.AddMessage(new Message(Message.Style.FADE, "材料不足！", false));
                return;
              }
            }
            foreach (var item in passiveSkillPhase.updateRequestItems) {
              DataManager.ConsumeItem(item.id, item.count);
            }
            DataManager.UpdateCharPassiveSkill(m_character);
            _RefreshDetail();
          }
        });
    }
  }
}