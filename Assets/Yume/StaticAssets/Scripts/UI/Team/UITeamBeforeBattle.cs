using System;
using UnityEngine;
using UnityEngine.UI;

namespace Yume.UI {
  public class UITeamBeforeBattle : UITeam {
    private GameObject m_team;
    private GameObject m_customTeam;
    private Button m_customCharBtn;
    private Button[] m_customCardBtns;
    private LevelTemplate m_levelTemplate;

    public override void OnInit(UITemplate uiTemplate, GameObject gameObject) {
      base.OnInit(uiTemplate, gameObject);

      m_team = GetGameObject("team");
      m_customTeam = GetGameObject("custom team");
      m_customCharBtn = GetComponent<Button>("custom char");
      m_customCardBtns = GetGameObject("custom cards").GetComponentsInChildren<Button>();

      _AddListener();
    }

    private void _AddListener() {
      GetComponent<Button>("start battle").onClick.AddListener(
        () => {
          if (!m_levelTemplate.isCustomTeam && string.IsNullOrEmpty(currentTeam.charId)) {
            MessageManager.AddMessage(new Message(Message.Style.CENTER, "未携带角色！", false, null, 0.5f));
            return;
          }

          UIManager.OpenOne(
            "globalBlackFade", () => {
              TeamArgs teamArgs;
              if (m_levelTemplate.isCustomTeam) {
                teamArgs = m_levelTemplate.customTeam;
              } else {
                UnitArgs charArgs = UnitArgs.Create(DataManager.GetCharacter(currentTeam.charId));
                CardArgs[] cardArgs = new CardArgs[currentTeam.cardGuids.Count];
                for (int i = 0; i < cardArgs.Length; i++) {
                  cardArgs[i] = CardArgs.Create(DataManager.GetCard(currentTeam.cardGuids[i]));
                }
                teamArgs = new TeamArgs(charArgs, cardArgs);
              }
              Action action = () => UIManager.DestroyOneAndOpenOne("single_battle", null, m_levelTemplate, teamArgs);
              if (string.IsNullOrEmpty(m_levelTemplate.beforeAVGId)) {
                action();
              } else {
                UIManager.OpenOne("avg", null, m_levelTemplate.beforeAVGId, action);
              }
            }
          );
        }
      );
    }

    public override void Open(Action callback = null, params object[] args) {
      m_levelTemplate = (LevelTemplate)args[0];
      if (m_levelTemplate.isCustomTeam) {
        m_team.SetActiveEx(false);
        m_customTeam.SetActiveEx(true);

        UnitArgs charArgs = m_levelTemplate.customTeam.unitArgs;
        UnitTemplate charTemplate = ResourceManager.Load<UnitTemplate>(charArgs.id);
        m_customCharBtn.image.SetSpriteAsync(charTemplate.portraitFileName);
        for (int i = 0; i < m_customCardBtns.Length; i++) {
          if (i < m_levelTemplate.customTeam.cardArgsArray.Length) {
            CardArgs cardArgs = m_levelTemplate.customTeam.cardArgsArray[i];
            CardTemplate cardTemplate = ResourceManager.Load<CardTemplate>(cardArgs.id);
            CardPhase cardPhase = cardTemplate.cardPhases[cardArgs.phase];
            m_customCardBtns[i].image.SetSpriteAsync(cardPhase.iconFileName);
          } else {
            m_customCardBtns[i].image.sprite = ResourceManager.Load<Sprite>("noCardSprite");
          }
        }
      } else {
        m_team.SetActiveEx(true);
        m_customTeam.SetActiveEx(false);
      }

      base.Open(callback, args);
    }
  }
}