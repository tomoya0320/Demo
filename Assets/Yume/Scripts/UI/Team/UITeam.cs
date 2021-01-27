using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Yume.Data;
using Yume.Resource;

namespace Yume.UI {
  public class UITeam : UIBase {
    private TeamData m_curTeam {
      get {
        if (m_curTeamIdx >= 0 && m_curTeamIdx < m_teams.Count) {
          return m_teams[m_curTeamIdx];
        }
        return null;
      }
    }
    private Button m_charBtn;
    private List<TeamData> m_teams;
    private int m_curTeamIdx = -1;
    private Sprite m_noCharSprite;
    private Sprite m_noCardSprite;
    private List<Button> m_cardBtns;
    private List<YButton> m_teamBtns;

    public override void OnInit(UITemplate uiTemplate, GameObject root) {
      base.OnInit(uiTemplate, root);

      m_charBtn = GetComponent<Button>("char");
      m_cardBtns = new List<Button>(GetGameObject("cards").GetComponentsInChildren<Button>());
      m_teamBtns = new List<YButton>(GetGameObject("team_buttons").GetComponentsInChildren<YButton>());
      m_noCharSprite = ResourceManager.Load<Sprite>("noCharSprite");
      m_noCardSprite = ResourceManager.Load<Sprite>("noCardSprite");

      m_teams = DataManager.userTeams;
      m_curTeamIdx = DataManager.userTeamIndex;
      m_teamBtns[m_curTeamIdx].image.color = Color.blue;

      _AddListener();
      _OnRefreshChar();
      _OnRefreshCards();
    }

    private void _AddListener() {
      for (int i = 0; i < m_teams.Count; i++) {
        TeamData team = m_teams[i];
        m_teamBtns[i].textComponent.text = team.name;
        m_teamBtns[i].onClick.AddListener(() => _SwitchTeam(team));
      }

      Action<string> onCharSelected = selectedCharId => {
        DataManager.ChangeTeamChar(m_curTeam, selectedCharId);
        _OnRefreshChar();
      };
      m_charBtn.onClick.AddListener(() => {
        UIManager.OpenOne("char_choice", null, m_curTeam.charId, onCharSelected);
      });

      Action<List<int>> onCardSelected = selectedCardIds => {
        DataManager.ChangeTeamCards(m_curTeam, selectedCardIds);
        _OnRefreshCards();
      };
      foreach (var cardBtn in m_cardBtns) {
        cardBtn.onClick.AddListener(() => {
          UIManager.OpenOne("card_choice", null, m_curTeam.cardGuidList, onCardSelected);
        });
      }

      GetComponent<Button>("close").onClick.AddListener(() => UIManager.CloseOne(name));
    }

    private void _SwitchTeam(TeamData teamDB) {
      int newIdx = m_teams.FindIndex(t => t == teamDB);

      if (m_curTeamIdx != newIdx) {
        TeamData lastTeam = m_curTeam;
        m_teamBtns[m_curTeamIdx].image.color = Color.white;
        m_curTeamIdx = newIdx;
        m_teamBtns[m_curTeamIdx].image.color = Color.blue;

        DataManager.ChangeTeamIndex(m_curTeamIdx);

        _OnRefreshChar();
        _OnRefreshCards();
      } else {
        Action<string> onEditCompleted = text => {
          DataManager.ChangeTeamName(m_curTeam, text);
          m_teamBtns[m_curTeamIdx].textComponent.text = text;
        };
        UIManager.OpenOne("text_editor", null, m_curTeam.name, onEditCompleted);
      }
    }

    private void _OnRefreshCards() {
      for (int i = 0; i < m_cardBtns.Count; i++) {
        if (i < m_curTeam.cardGuidList.Count) {
          CardData card = DataManager.GetCard(m_curTeam.cardGuidList[i]);
          CardTemplate cardTemplate = ResourceManager.Load<CardTemplate>(card.id);
          CardPhase cardPhase = cardTemplate.cardPhases[card.phase];
          m_cardBtns[i].image.sprite = ResourceManager.Load<Sprite>(cardPhase.iconFileName);
        } else {
          m_cardBtns[i].image.sprite = m_noCardSprite;
        }
      }
    }

    private void _OnRefreshChar() {
      if (!string.IsNullOrEmpty(m_curTeam.charId)) {
        UnitTemplate charTemplate = ResourceManager.Load<UnitTemplate>(m_curTeam.charId);
        m_charBtn.image.sprite = ResourceManager.Load<Sprite>(charTemplate.portraitFileName);
      } else {
        m_charBtn.image.sprite = m_noCharSprite;
      }
    }
  }
}