using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yume.Audio;
using Yume.Broadcast;
using Yume.Data;
using Yume.UI;
using EventType = Yume.Broadcast.EventType;

namespace Yume.Battle {
  public enum BattleResult {
    WIN, // 战斗胜利
    LOSE, // 战斗失败
    ABORT, // 放弃战斗
  }

  public static class BattleManager {
    private static Unit s_lastUnit;
    private static Coroutine s_coroutine;
    private static int s_currentUnitIndex;
    private static LevelTemplate s_levelTemplate;
    private static List<Unit> s_units = new List<Unit>();
    public static List<Unit> units => s_units.FindAll(unit => unit.isAlive);
    // 当前回合的单位
    public static Unit currentUnit => s_units[s_currentUnitIndex];
    // 玩家角色设定为放在第一个位置上
    public static Character character => s_units.First() as Character;
    public static Blackboard blackboard = new Blackboard();
    public static Dictionary<Unit, BuffManager> buffManagers = new Dictionary<Unit, BuffManager>();
    public static Dictionary<Unit, BehaviorManager> behaviorManagers = new Dictionary<Unit, BehaviorManager>();
    public static Action abort => () => Finish(BattleResult.ABORT);

    public static void Init(LevelTemplate levelTemplate, ObjectContainer objectContainer) {
      // 初始化工厂
      NumFactory.Init(objectContainer);
      UnitFactory.Init(objectContainer);
      CardFactory.Init(objectContainer);

      s_levelTemplate = levelTemplate;
      // 初始化角色
      var team = DataManager.userTeams[DataManager.userTeamIndex];
      var tempChar = DataManager.GetCharacter(team.charId);
      var cardList = new List<CardData>();
      foreach (var cardGuid in team.cardGuidList) {
        cardList.Add(DataManager.GetCard(cardGuid).Clone() /*因为是引用类型会影响到原来的数据所以需要复制一份*/);
      }
      s_units.Add(UnitFactory.CreateUnit<Character>(tempChar, cardList.ToArray()));
      // 初始化敌人
      foreach (var enemyTemplate in s_levelTemplate.enemyTemplates) {
        s_units.Add(UnitFactory.CreateUnit<Enemy>(enemyTemplate.enemy, enemyTemplate.cards));
      }
      // 初始化BuffManagers BehaviorManagers
      foreach (var unit in s_units) {
        buffManagers.Add(unit, new BuffManager(unit));
        behaviorManagers.Add(unit, new BehaviorManager(unit));
      }
      // TODO:初始化单位的被动Buff 有待调整
      foreach (var unit in s_units) {
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
    }

    public static void StartBattle() {
      // 播放战斗中的bgm
      AudioManager.PlayBgm(s_levelTemplate.bgm);
      // 开始作战
      s_coroutine = CoroutineHelper.instance.Run(_Run());
    }

    public static Unit GetNearestEnemy(Vector3 point) {
      Unit nearestEnemy = null;
      float minDist = float.MaxValue;
      foreach (var unit in units) {
        if (unit is Enemy) {
          float dist = Vector3.Distance(point, unit.gameObject.transform.position);
          if (dist < minDist) {
            nearestEnemy = unit;
            minDist = dist;
          }
        }
      }
      return nearestEnemy;
    }

    public static void Finish(BattleResult battleResult) {
      // 停止协程
      CoroutineHelper.instance.Stop(s_coroutine);
      s_coroutine = null;

      // TODO:战斗结算 测试用
      Action onExitBattle = _CleanUp;
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
        default:
          Debug.LogError("战斗结果为空！");
          return;
      }

      // 退出战斗界面
      UIManager.OpenOne(
        "globalBlackFade", () => { UIManager.CloseOne("single_battle", onExitBattle); }
      );
    }

    private static void _CleanUp() {
      // 清理
      s_units.ForEach(unit => unit.CleanUp());
      s_units.Clear();

      s_lastUnit = null;
      s_currentUnitIndex = 0;
      s_levelTemplate = default;

      blackboard.Clear();
      buffManagers.Clear();
      behaviorManagers.Clear();
      
      NumFactory.CleanUp();
      UnitFactory.CleanUp();
      CardFactory.CleanUp();
    }

    private static IEnumerator _Run() {
      while (true) {
        // 更新UI
        if (currentUnit is Character) {
          EventCenter.Broadcast(EventType.ON_ENTER_PLAYER_TURN);
        } else if (s_lastUnit is Character) {
          EventCenter.Broadcast(EventType.ON_ENTER_FIRST_ENEMY_TURN);
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
        // 下一个存活的单位
        s_lastUnit = currentUnit;
        do {
          s_currentUnitIndex = (s_currentUnitIndex + 1) % s_units.Count;
        } while (!currentUnit.isAlive);
      }
    }
  }
}