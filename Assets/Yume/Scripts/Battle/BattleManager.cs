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
  public enum BattleState {
    NONE, // 空
    RUNNING, // 战斗进行中
    WIN, // 战斗胜利
    LOSE, // 战斗失败
    ABORT, // 放弃战斗
  }

  public static class BattleManager {
    private static int s_currentUnitIndex;
    private static LevelTemplate s_levelTemplate;
    public static Unit character => units.First();
    public static List<Unit> units = new List<Unit>();
    public static Blackboard blackboard = new Blackboard();
    public static BattleState battleState = BattleState.NONE;
    public static bool isRunning => battleState == BattleState.RUNNING;
    public static Dictionary<Unit, BuffManager> buffManagers = new Dictionary<Unit, BuffManager>();
    public static Dictionary<Unit, BehaviorManager> behaviorManagers = new Dictionary<Unit, BehaviorManager>();
    public static Action abort => () => {
      if (battleState == BattleState.RUNNING) {
        battleState = BattleState.ABORT;
      } else {
        Debug.LogError("战斗未运行！");
      }
    };
    
#if BETA
    private static Text s_log;
    public static void Log(string content) {
      s_log.text += $"\n[{DateTime.Now.ToLongTimeString()}]:{content}";
    }
#endif

    public static void Init(LevelTemplate levelTemplate) {
      // 初始化工厂
      UnitFactory.Init();
      CardFactory.Init();
      
      s_levelTemplate = levelTemplate;
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
      // 初始化BuffManagers BehaviorManagers
      foreach (var unit in units) {
        buffManagers.Add(unit, new BuffManager(unit));
        behaviorManagers.Add(unit, new BehaviorManager(unit));
      }
      // TODO:初始化单位的被动Buff 有待调整
      foreach (var unit in units) {
        if (unit.passiveSkillLevel >= 0 && unit.passiveSkillLevel < unit.passiveSkillPhases.Length) {
          foreach (var buffTemplate in unit.passiveSkillPhases[unit.passiveSkillLevel].buffTemplates) {
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
      battleState = BattleState.RUNNING;
      CoroutineHelper.instance.Run(_Run());
    }

    public static Unit GetNearestEnemy(Vector3 point) {
      int index = 0;
      float minDist = float.MaxValue;
      for (int i = 1; i < units.Count; i++) {
        float dist = Vector3.Distance(point, units[i].gameObject.transform.position);
        if (dist < minDist) {
          index = i;
          minDist = dist;
        }
      }
      return index > 0 ? units[index] : null;
    }

    private static void _Finish() {
      // TODO:战斗结算 测试用
      Action onExitBattle = () => AudioManager.PlayBgm(StringMap.DEFAULT_BGM);
      switch (battleState) {
        case BattleState.WIN:
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
        case BattleState.LOSE:
          MessageManager.AddMessage(new Message(Message.Style.FADE, "作战失败"));
          break;
        case BattleState.ABORT:
          MessageManager.AddMessage(new Message(Message.Style.FADE, "放弃作战"));
          break;
        case BattleState.NONE:
          Debug.LogError("战斗状态为空！");
          return;
      }

      // 清理
      units.ForEach(unit => unit.CleanUp());
      units.Clear();

      s_currentUnitIndex = 0;
      s_levelTemplate = default;

      blackboard.Clear();
      buffManagers.Clear();
      behaviorManagers.Clear();

      battleState = BattleState.NONE;
      
      UnitFactory.CleanUp();
      CardFactory.CleanUp();

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
        // 检测战斗是否还在运行
        if (battleState != BattleState.RUNNING) {
          _Finish();
          yield break;
        }
        // 下一个单位
        s_currentUnitIndex = (s_currentUnitIndex + 1) % units.Count;
      }
    }
  }
}