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
    public string name = "默认队伍名";
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
    private Sprite m_noCharSprite;
    private Sprite m_noCardSprite;
    private List<Button> m_cardBtns;
    private List<YButton> m_teamBtns;

    public override void OnInit(UIData uiData, GameObject root) {
      base.OnInit(uiData, root);

      m_charBtn = GetComponent<Button>("char");
      m_cardBtns = new List<Button>(GetGameObject("cards").GetComponentsInChildren<Button>());
      m_teamBtns = new List<YButton>(GetGameObject("team_buttons").GetComponentsInChildren<YButton>());
      m_noCharSprite = AssetLoader.instance.GetValue<Sprite>("noCharSprite");
      m_noCardSprite = AssetLoader.instance.GetValue<Sprite>("noCardSprite");

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

    private void _SwitchTeam(TeamDB teamDB) {
      int newIdx = m_teams.FindIndex(t => t == teamDB);

      if (m_curTeamIdx != newIdx) {
        if (m_curTeam != null) {
          m_curTeam.isSelected = false;
          m_teamBtns[m_curTeamIdx].image.color = Color.white;
          m_charBtn.onClick.RemoveAllListeners();
          for (int i = 0; i < m_cardBtns.Count; i++) {
            m_cardBtns[i].onClick.RemoveAllListeners();
          }
        }

        m_curTeamIdx = newIdx;
        m_curTeam.isSelected = true;
        m_teamBtns[m_curTeamIdx].image.color = Color.blue;

        for (int i = 0; i < m_teams.Count; i++) {
          m_teamBtns[i].textComponent.text = m_teams[i].name;
        }

        _OnRefreshChar();
        _OnRefreshCards();

        Action<string> onCharSelected = selectedCharId => {
          if(m_curTeam.charId != selectedCharId) {
            m_curTeam.charId = selectedCharId;
            _OnRefreshChar();
            DBManager.instance.UpdateUserData();
          }
        };
        m_charBtn.onClick.AddListener(() => {
          UnitDB unitDB = DBManager.instance.GetCharDB(m_curTeam.charId);
          UIManager.instance.OpenOne("char_choice", null, unitDB, onCharSelected);
        });

        //TODO:卡牌选择事件监听注册

        DBManager.instance.UpdateUserData();
      } else {
        Action<string> onEditCompleted = text => {
          if (m_curTeam.name != text) {
            m_curTeam.name = text;
            m_teamBtns[m_curTeamIdx].textComponent.text = text;
            DBManager.instance.UpdateUserData();
          }
        };
        UIManager.instance.OpenOne("text_editor", null, m_curTeam.name, onEditCompleted);
      }
    }

    private async void _OnRefreshCards() {
      for (int i = 0; i < m_curTeam.cardIdList.Count; i++) {
        string cardId = m_curTeam.cardIdList[i];
        if (!string.IsNullOrEmpty(cardId)) {
          CardDB cardDB = DBManager.instance.GetCardDB(cardId);
          CardInfo cardInfo = DBManager.instance.GetCardInfo(cardId);
          CardPhase cardPhase = cardInfo.cardPhases[cardDB.phase];
          m_cardBtns[i].image.sprite = await ResourceManager.LoadAssetAsync<Sprite>(cardPhase.iconFileName);
        }
      }
    }

    private async void _OnRefreshChar() {
      if (!string.IsNullOrEmpty(m_curTeam.charId)) {
        UnitInfo charInfo = DBManager.instance.GetUnitInfo(m_curTeam.charId);
        m_charBtn.image.sprite = await ResourceManager.LoadAssetAsync<Sprite>(charInfo.portraitFileName);
      } else {
        m_charBtn.image.sprite = m_noCharSprite;
      }
    }
  }
}