using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.Utilities;
using UnityEngine;
using Yume.Broadcast;
using Yume.Data;
using Yume.UI;
using EventType = Yume.Broadcast.EventType;

namespace Yume.Battle {
  public enum BattleResult {
    NONE, WIN, LOSE, ABORT
  }

  public static class BattleManager {
    private static int s_currentUnitIndex;
    private static Coroutine s_runCoroutine;
    private static LevelTemplate s_levelTemplate;
    private static List<Unit> s_units = new List<Unit>();
    public static Blackboard blackboard = new Blackboard();
    // TODO:测试用
    public static bool isRunning => s_runCoroutine != null;
    // TODO:测试用
    public static BattleResult battleResult = BattleResult.NONE;
    public static Dictionary<Unit, BuffManager> buffManagers = new Dictionary<Unit, BuffManager>();
    public static Dictionary<Unit, BehaviorManager> behaviorManagers = new Dictionary<Unit, BehaviorManager>();
    // TODO:测试用
    public static Action abort = () => battleResult = BattleResult.ABORT;
    // TODO:测试用
    public static Unit enemy => s_units.Count > 1 ? s_units[1] : null;
    public static Unit character => s_units.Count > 0 ? s_units[0] : null;
    private static Dictionary<string, Transform> s_folderDict = new Dictionary<string, Transform>();
    public static Transform cardFolder => s_folderDict.ContainsKey("cardFolder") ? s_folderDict["cardFolder"] : null;
    public static Transform charFolder => s_folderDict.ContainsKey("charFolder") ? s_folderDict["charFolder"] : null;
    public static Transform enemyFolder => s_folderDict.ContainsKey("enemyFolder") ? s_folderDict["enemyFolder"] : null;

    public static void Init(LevelTemplate levelTemplate, Dictionary<string, Transform> folderDict) {
      s_levelTemplate = levelTemplate;
      // 初始化folder
      foreach (var kv in folderDict) {
        s_folderDict.Add(kv.Key, kv.Value);
      }
      // 初始化角色
      var team = DataManager.userTeams[DataManager.userTeamIndex];
      var tempChar = DataManager.GetCharacter(team.charId);
      var cardList = new List<CardData>();
      foreach (var cardGuid in team.cardGuidList) {
        cardList.Add(DataManager.GetCard(cardGuid).Clone() /*因为是引用类型会影响到原来的数据所以需要复制一份*/);
      }
      s_units.Add(new Character(tempChar, cardList.ToArray()));
      // 初始化敌人
      foreach (var enemyTemplate in s_levelTemplate.enemyTemplates) {
        s_units.Add(new Enemy(enemyTemplate.enemy, enemyTemplate.cards));
      }
      // 初始化AttribCaches BuffManagers BehaviorManagers
      foreach (var unit in s_units) {
        var buffManager = new BuffManager(unit);
        if (unit.passiveSkillLevel >= 0 && unit.passiveSkillLevel < unit.passiveSkillPhases.Length) {
          // 被动技能buff
          var passiveSkill = unit.passiveSkillPhases[unit.passiveSkillLevel];
          buffManager.AddBuff(passiveSkill.buffTemplateId, passiveSkill.blackboard);
        }
        buffManagers.Add(unit, buffManager);
        
        behaviorManagers.Add(unit, new BehaviorManager(unit));
      }
      // TODO:初始化单位的被动Buff
    }

    public static void StartBattle() {
      // 开始作战
      s_runCoroutine = CoroutineHelper.instance.Run(_Run());
    }

    private static void _Finish() {
      if (s_runCoroutine == null) {
        Debug.LogError("战斗尚未开始！");
        return;
      }
      
      // TODO:战斗结算 测试用
      switch (battleResult) {
        case BattleResult.WIN:
          MessageManager.AddMessage(new Message(Message.Style.FADE, "作战胜利"));
          DataManager.CompleteLevel(s_levelTemplate.id);
          break;
        case BattleResult.LOSE:
          MessageManager.AddMessage(new Message(Message.Style.FADE, "作战失败"));
          break;
        case BattleResult.ABORT:
          MessageManager.AddMessage(new Message(Message.Style.FADE, "放弃作战"));
          break;
        case BattleResult.NONE:
          Debug.LogError("战斗结果为空！");
          break;
      }

      // 清理
      s_runCoroutine = null;
      s_units.ForEach(unit => unit.CleanUp());
      s_units.Clear();
      s_folderDict.Clear();

      s_currentUnitIndex = 0;
      s_levelTemplate = default;

      blackboard.Clear();
      buffManagers.Clear();
      behaviorManagers.Clear();

      battleResult = BattleResult.NONE;
      
      // 退出战斗界面
      UIManager.OpenOne("globalBlackFade",()=>UIManager.CloseOne("single_battle"));
    }

    private static IEnumerator _Run() {
      while (true) {
        // 回合开始前
        behaviorManagers.Values.ForEach(buffManager => buffManager.Run(BehaviorTime.ON_BEFORE_TURN));
        switch (s_currentUnitIndex) {
          // 默认玩家是第一个位置
          case 0:
            // 进入玩家的回合
            EventCenter.Broadcast(EventType.ON_ENTER_PLAYER_TURN);
            break;
          case 1:
            // 进入第一个敌人的回合
            EventCenter.Broadcast(EventType.ON_ENTER_FIRST_ENEMY_TURN);
            break;
        }
        yield return s_units[s_currentUnitIndex].OnTrick();
        // 下一个单位
        s_currentUnitIndex = (s_currentUnitIndex + 1) % s_units.Count;
        // 更新buff
        buffManagers.Values.ForEach(buffManager => buffManager.OnLateUpdate());
        // 回合结束后
        behaviorManagers.Values.ForEach(buffManager => buffManager.Run(BehaviorTime.ON_LATE_TURN));
        // TODO:测试用 检测战斗是否完成
        if (battleResult != BattleResult.NONE) {
          _Finish();
          yield break;
        }
      }
    }
  }
}