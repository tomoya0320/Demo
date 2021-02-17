using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Yume.Audio;
using Yume.Broadcast;
using Yume.Data;
using Yume.UI;
using EventType = Yume.Broadcast.EventType;

namespace Yume.Battle {
  public enum BattleResult {
    NONE,
    WIN,
    LOSE,
    ABORT
  }

  public static class BattleManager {
    private static int s_currentUnitIndex;
    private static LevelTemplate s_levelTemplate;
    public static Blackboard blackboard = new Blackboard();
    public static List<Unit> units { get; } = new List<Unit>();
    // TODO:测试用
    public static bool isRunning => battleResult == BattleResult.NONE;
    // TODO:测试用
    public static BattleResult battleResult = BattleResult.NONE;
    public static Dictionary<Unit, BuffManager> buffManagers = new Dictionary<Unit, BuffManager>();
    public static Dictionary<Unit, BehaviorManager> behaviorManagers = new Dictionary<Unit, BehaviorManager>();
    // TODO:测试用
    public static Action abort = () => battleResult = BattleResult.ABORT;
    // TODO:测试用 一场战斗暂时只有一个敌人
    public static Unit enemy => units.Count > 1 ? units[1] : null;
    public static Unit character => units.Count > 0 ? units[0] : null;
    private static Dictionary<string, Transform> s_folderDict = new Dictionary<string, Transform>();
    public static Transform cardFolder => s_folderDict.ContainsKey("cardFolder") ? s_folderDict["cardFolder"] : null;
    public static Transform charFolder => s_folderDict.ContainsKey("charFolder") ? s_folderDict["charFolder"] : null;
    public static Transform enemyFolder => s_folderDict.ContainsKey("enemyFolder") ? s_folderDict["enemyFolder"] : null;
#if BETA
    private static Text s_log;
    public static void Log(string content) {
      s_log.text += $"\n[{DateTime.Now.ToLongTimeString()}]:{content}";
    }
#endif

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
      units.Add(new Character(tempChar, cardList.ToArray()));
      // 初始化敌人
      foreach (var enemyTemplate in s_levelTemplate.enemyTemplates) {
        units.Add(new Enemy(enemyTemplate.enemy, enemyTemplate.cards));
      }
      // 初始化AttribCaches BuffManagers BehaviorManagers
      foreach (var unit in units) {
        buffManagers.Add(unit, new BuffManager(unit));
        behaviorManagers.Add(unit, new BehaviorManager(unit));
      }
      // TODO:初始化单位的被动Buff 有待调整
      foreach (var unit in units) {
        if (unit.passiveSkillLevel >= 0 && unit.passiveSkillLevel < unit.passiveSkillPhases.Length) {
          foreach (var buffTemplate in unit.passiveSkillPhases[unit.passiveSkillLevel].buffTemplates) {
            // TODO:筛选出目标 测试用
            List<Unit> targets = buffTemplate.selector.Select(unit, null);
            foreach (var target in targets) {
              // 因为blackboard是引用类型所以需要新建一个
              Blackboard blackboard = new Blackboard { { "caster", unit } };
              blackboard = blackboard.Merge(buffTemplate.blackboard);
              buffManagers[target].AddBuff(buffTemplate.id, buffTemplate.stackType, blackboard, buffTemplate.behaviors);         
            }
          }
        }
      }
      
#if BETA
      s_log = GameObject.Find("log").GetComponent<Text>();
      s_log.text = "行动日志=============================";
#endif
    }

    public static void StartBattle() {
      // 开始作战
      CoroutineHelper.instance.Run(_Run());
    }

    private static void _Finish() {
      // TODO:战斗结算 测试用
      Action onExitBattle = () => AudioManager.PlayBgm(StringMap.DEFAULT_BGM);
      switch (battleResult) {
        case BattleResult.WIN:
          // 避免引用到BattleManager的变量 因为这个变量后面马上就置空了
          LevelTemplate levelTemplate = s_levelTemplate;
          if (!string.IsNullOrEmpty(levelTemplate.afterAVGId)) {
            Action action = () => {
              MessageManager.AddMessage(new Message(Message.Style.FADE, "作战胜利"));
              DataManager.CompleteLevel(levelTemplate.id);
            };
            onExitBattle += () => UIManager.OpenOne("avg", null, levelTemplate.afterAVGId, action);
          }
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
      units.ForEach(unit => unit.CleanUp());
      units.Clear();
      s_folderDict.Clear();

      s_currentUnitIndex = 0;
      s_levelTemplate = default;

      blackboard.Clear();
      buffManagers.Clear();
      behaviorManagers.Clear();

      battleResult = BattleResult.NONE;

      // 退出战斗界面
      UIManager.OpenOne(
        "globalBlackFade", () => {
          UIManager.CloseOne("single_battle", onExitBattle);
        }
      );
    }

    private static IEnumerator _Run() {
      // 播放战斗中的bgm
      AudioManager.PlayBgm(s_levelTemplate.bgm);

      while (true) {
        Unit currentUnit = units[s_currentUnitIndex];
        // 更新UI
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
        // 补充能量
        float energy = currentUnit.GetAttrib(AttribType.ENERGY);
        float maxEnergy = currentUnit.GetAttrib(AttribType.ENERGY, true);
        if (energy < maxEnergy) {
          currentUnit.AddAttrib(AttribType.ENERGY, maxEnergy - energy);
        }
        // 回合开始前
        behaviorManagers[currentUnit].Run(BehaviorTime.ON_BEFORE_TURN);
        // 回合中
        yield return currentUnit.OnTick();
        // 回合结束后
        behaviorManagers[currentUnit].Run(BehaviorTime.ON_LATE_TURN);
        // 更新buff
        buffManagers[currentUnit].OnLateUpdate();
        // TODO:测试用 检测战斗是否完成
        if (battleResult != BattleResult.NONE) {
          _Finish();
          yield break;
        }
        // 下一个单位
        s_currentUnitIndex = (s_currentUnitIndex + 1) % units.Count;
      }
    }
  }
}