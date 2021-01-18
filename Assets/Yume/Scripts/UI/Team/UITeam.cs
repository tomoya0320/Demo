using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Yume.Battle;
using Yume.DB;
using Yume.Resource;

namespace Yume.UI {
  [Serializable]
  public class TeamDB {
    public string charId;
    public bool isSelected;
    public string name = "Ä¬ÈÏ¶ÓÎéÃû";
    public List<string> cardIdList = new List<string>();
  }

  public class UITeam : UIBase {
    private TeamDB m_curTeam {
      get {
        if (m_curTeamIdx >= 0 && m_curTeamIdx < m_teams.Count) {
          return m_teams[m_curTeamIdx];
        }
        return null;
      }
    }
    private Button m_charBtn;
    private List<TeamDB> m_teams;
    private int m_curTeamIdx = -1;
    private List<Button> m_cardBtns;
    private List<YButton> m_teamBtns;

    public override void OnInit(UIData uiData, GameObject root) {
      base.OnInit(uiData, root);

      m_charBtn = GetComponent<Button>("char");
      m_cardBtns = new List<Button>(GetGameObject("cards").GetComponentsInChildren<Button>());
      m_teamBtns = new List<YButton>(GetGameObject("team_buttons").GetComponentsInChildren<YButton>());

      m_teams = DBManager.instance.userTeams;

      _SwitchTeam(m_teams.Find(t => t.isSelected));

      _AddListener();
    }

    private void _AddListener() {
      for (int i = 0; i < m_teams.Count; i++) {
        TeamDB teamDB = m_teams[i];
        m_teamBtns[i].onClick.AddListener(() => _SwitchTeam(teamDB));
      }
      GetComponent<Button>("close").onClick.AddListener(() => UIManager.instance.CloseOne(name));
    }

    private async void _SwitchTeam(TeamDB teamDB) {
      int newIdx = m_teams.FindIndex(t => t == teamDB);

      if (m_curTeamIdx != newIdx) {
        if (m_curTeam != null) {
          m_curTeam.isSelected = false;
          m_teamBtns[m_curTeamIdx].image.color = Color.white;
        }

        m_curTeamIdx = newIdx;
        m_curTeam.isSelected = true;
        m_teamBtns[m_curTeamIdx].image.color = Color.blue;

        for (int i = 0; i < m_teams.Count; i++) {
          m_teamBtns[i].textComponent.text = m_teams[i].name;
        }

        string charId = m_curTeam.charId;
        if (!string.IsNullOrEmpty(charId)) {
          UnitInfo charInfo = DBManager.instance.GetUnitInfo(charId);
          m_charBtn.image.sprite = await ResourceManager.LoadAssetAsync<Sprite>(charInfo.portraitFileName);
        }

        for (int i = 0; i < m_curTeam.cardIdList.Count; i++) {
          string cardId = m_curTeam.cardIdList[i];
          if (!string.IsNullOrEmpty(cardId)) {
            CardDB cardDB = DBManager.instance.GetCardDB(cardId);
            CardInfo cardInfo = DBManager.instance.GetCardInfo(cardId);
            CardPhase cardPhase = cardInfo.cardPhases[cardDB.phase];
            m_cardBtns[i].image.sprite = await ResourceManager.LoadAssetAsync<Sprite>(cardPhase.iconFileName);
          }
        }

        DBManager.instance.UpdateUserData();
      } else {
        Action<string> onEditCompleted = text => {
          if (m_curTeam.name != text) {
            m_curTeam.name = text;
            DBManager.instance.UpdateUserData();
          }
        };
        UIManager.instance.OpenOne("text_editor", null, onEditCompleted);
      }
    }
  }
}