using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yume.Data;
using Yume.Tool;
using Yume.UI;

namespace Yume.Battle {
  public enum BattleResult {
    WIN, LOSE, ABORT
  }

  public static class BattleManager {
    private static int s_currentUnitIndex;
    private static Coroutine s_runCoroutine;
    private static LevelTemplate s_levelTemplate;
    private static List<Unit> s_units = new List<Unit>();
    public static Action abort = () => Finish(BattleResult.ABORT);
    public static Unit character => s_units.Count > 0 ? s_units[0] : null;
    private static Dictionary<string, Transform> m_folderDict = new Dictionary<string, Transform>();
    public static Transform charFolder => m_folderDict["charFolder"];
    public static Transform enemyFolder => m_folderDict["enemyFolder"];
    public static Transform drawCardFolder => m_folderDict["drawCardFolder"];
    public static Transform discardCardFolder => m_folderDict["discardCardFolder"];
    public static Transform consumeCardFolder => m_folderDict["consumeCardFolder"];

    public static void Init(LevelTemplate levelTemplate, Dictionary<string, Transform> folderDict) {
      s_levelTemplate = levelTemplate;
      // 初始化folder
      foreach (var kv in folderDict) {
        m_folderDict.Add(kv.Key, kv.Value);
      }
      // 初始化角色
      TeamData team = DataManager.userTeams[DataManager.userTeamIndex];
      UnitData character = DataManager.GetCharacter(team.charId);
      List<CardData> cardList = new List<CardData>();
      foreach (var cardGuid in team.cardGuidList) {
        cardList.Add(DataManager.GetCard(cardGuid).Clone()/*因为是引用类型会影响到原来的数据所以需要复制一份*/);
      }
      s_units.Add(new Character(character, cardList));
      // 初始化敌人
      foreach (var enemyTemplate in s_levelTemplate.enemyTemplates) {
        s_units.Add(new Enemy(enemyTemplate.enemy, enemyTemplate.cards));
      }
    }

    public static void StartBattle() {
      // 开始作战
      s_runCoroutine = CoroutineHelper.instance.Run(_Run());
    }

    public static void Finish(BattleResult result) {
      if(s_runCoroutine == null) {
        Debug.LogError("战斗尚未开始！");
        return;
      }

      // 停止运行协程
      CoroutineHelper.instance.Stop(s_runCoroutine);
      s_runCoroutine = null;

      s_units.Clear();
      m_folderDict.Clear();
      s_currentUnitIndex = 0;
      s_levelTemplate = default;

      // TODO:战斗结算
      switch (result) {
        case BattleResult.WIN:
          break;
        case BattleResult.LOSE:
          break;
        case BattleResult.ABORT:
          break;
      }
    }

    private static IEnumerator _Run() {
      while (true) {
        // 默认玩家是第一个位置
        if (s_currentUnitIndex == 0) {
          MessageManager.AddMessage(new Message(Message.Style.CENTER, "你的回合", null, 1.0f));
        }
        yield return s_units[s_currentUnitIndex].OnTrick();
        // 下一个单位
        s_currentUnitIndex = (s_currentUnitIndex + 1) % s_units.Count;
      }
    }
  }
}