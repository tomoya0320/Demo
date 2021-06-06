using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Sirenix.Utilities;
using UnityEngine;
using Yume.Broadcast;
using EventType = Yume.Broadcast.EventType;

namespace Yume.Battle {
  public enum BattleResult {
    [Description("作战胜利")]
    WIN,
    [Description("作战失败")]
    LOSE,
    [Description("放弃作战")]
    ABORT,
  }

  public static class BattleManager {
    private static Unit s_lastUnit;
    private static int s_currentUnitIndex;
    private static IEnumerator s_runEnumerator;
    private static LevelTemplate s_levelTemplate;
    public static List<Unit> units = new List<Unit>();
    public static bool isRunning => s_runEnumerator != null;
    public static Unit currentUnit => units[s_currentUnitIndex]; // 当前回合的单位
    public static Character character => units.First() as Character; // 玩家角色设定为放在第一个位置上
    public static Blackboard blackboard = new Blackboard();
    public static Dictionary<Unit, BuffManager> buffManagers = new Dictionary<Unit, BuffManager>();
    public static Dictionary<Unit, BehaviorManager> behaviorManagers = new Dictionary<Unit, BehaviorManager>();
    private const float LATE_TURN_WAITING_TIME = 0.5f;

    public static void StartBattle(TeamArgs teamArgs, LevelTemplate levelTemplate, ObjectContainer objectContainer) {
      // 初始化工厂
      NumFactory.Init(objectContainer);
      UnitFactory.Init(objectContainer);
      CardFactory.Init(objectContainer);

      s_levelTemplate = levelTemplate;
      // 初始化角色
      units.Add(UnitFactory.CreateUnit<Character>(teamArgs.unitArgs, teamArgs.cardArgsArray));
      // 初始化敌人
      foreach (var enemyTeam in s_levelTemplate.enemyTeams) {
        units.Add(UnitFactory.CreateUnit<Enemy>(enemyTeam.unitArgs, enemyTeam.cardArgsArray));
      }
      // 初始化BuffManagers BehaviorManagers
      foreach (var unit in units) {
        buffManagers.Add(unit, new BuffManager(unit));
        behaviorManagers.Add(unit, new BehaviorManager(unit));
      }
      // 初始化单位的被动技能
      foreach (var unit in units) {
        if (unit.passiveSkillLevel >= 0 && unit.passiveSkillLevel < unit.passiveSkillPhases.Length) {
          foreach (var buffTemplate in unit.passiveSkillPhases[unit.passiveSkillLevel].buffTemplates) {
            foreach (var target in buffTemplate.selector.Select(unit)) {
              // 因为blackboard是引用类型所以需要新建一个
              Blackboard blackboard = new Blackboard { { "caster", unit } };
              blackboard.Merge(buffTemplate.blackboard);
              buffManagers[target].AddBuff(
                buffTemplate.id, buffTemplate.stackType, blackboard, buffTemplate.behaviors, buffTemplate.display
              );
            }
          }
        }
      }
      // 初始化关卡的Blackboard
      blackboard.Merge(s_levelTemplate.blackboard);
      // 初始化关卡的Buff
      foreach (var buffTemplate in s_levelTemplate.buffTemplates) {
        foreach (var target in buffTemplate.selector.Select()) {
          // 因为blackboard是引用类型所以需要新建一个
          Blackboard blackboard = new Blackboard();
          blackboard.Merge(buffTemplate.blackboard);
          buffManagers[target].AddBuff(
            buffTemplate.id, buffTemplate.stackType, blackboard, buffTemplate.behaviors, buffTemplate.display
          );
        }
      }
      
      // 开始作战
      s_runEnumerator = _Run();
      CoroutineHelper.instance.Run(s_runEnumerator);
      // 注册每帧调用的更新事件
      Game.instance.onUpdate += _OnUpdate;
      // 进入战斗数据更新
      DataManager.OnEnterLevel(levelTemplate.id);
    }

    public static Unit GetNearestEnemy(Vector3 point) {
      Unit nearestEnemy = null;
      float minDist = float.MaxValue;
      foreach (var unit in units) {
        if (unit.isDead) {
          continue;
        }
        
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
      // 避免反复触发结算
      if (!isRunning) {
        return;
      }
      
      // 停止协程
      CoroutineHelper.instance.Stop(s_runEnumerator);
      s_runEnumerator = null;
      // 移除每帧调用的更新事件
      Game.instance.onUpdate -= _OnUpdate;

      // 避免引用到BattleManager的变量因为这些变量后面就置空了
      LevelTemplate levelTemplate = s_levelTemplate;
      // 退出战斗
      UIManager.GlobalFade(
        GlobalFadeType.BLACK_FADE, () => {
          // 先清理
          _CleanUp();
          // 再关闭战斗UI
          UIManager.CloseOneEx("single_battle");
          // 最后结算
          Action action = () => {
            MessageManager.AddMessage(new Message(Message.Style.FADE, battleResult.GetDescription()));
            if (battleResult == BattleResult.WIN) {
              DataManager.CompleteLevel(levelTemplate.id);
            }
          };
          // 判断胜利的情况下是否需要播放avg
          if (battleResult == BattleResult.WIN && !string.IsNullOrEmpty(levelTemplate.afterAVGId)) {
            UIManager.OpenOne("avg", null, levelTemplate.afterAVGId, action);
          } else {
            action();
          }
        }
      );
    }

    public static void OnUnitDie(Unit unit) {
      // 单位死亡的动画
      unit.animator.SetBool(AnimatorParams.DIE, true);
      
      // 移除这个单位施加的buff
      foreach (var buffManager in buffManagers.Values) {
        if (buffManager.unit.isDead) {
          continue;
        }
        
        buffManager.OnCasterDie(unit);
      }
      
      if (unit is Character) {
        // 战斗失败
        Finish(BattleResult.LOSE);
      } else if (unit is Enemy) {
        // 判断是否胜利
        if (units.FindAll(unit => unit.isAlive).Count == 1) {
          Finish(BattleResult.WIN);
        }
      }
    }

    private static void _OnUpdate() {
      foreach (var unit in units) {
        if (unit.isDead) {
          continue;
        }
        
        unit.OnUpdate();
      }
    }

    private static void _CleanUp() {
      // step1
      s_lastUnit = null;
      s_currentUnitIndex = 0;
      s_levelTemplate = default;
      
      // step2
      units.ForEach(unit => unit.CleanUp());
      buffManagers.Values.ForEach(buffManager => buffManager.CleanUp());
      behaviorManagers.Values.ForEach(behaviorManager => behaviorManager.CleanUp());

      // step3
      units.Clear();
      blackboard.Clear();
      buffManagers.Clear();
      behaviorManagers.Clear();

      // step4
      NumFactory.CleanUp();
      UnitFactory.CleanUp();
      CardFactory.CleanUp();
      
      // step5
      GC.Collect();
    }

    private static IEnumerator _Run() {
      // 等UI动画和全局过渡动画都结束了再运行
      YWaitWhile waitAllUI = WaitUtil.GetWaitWhile(() => UIManager.blockNormalUI || UIManager.globalFading);
      yield return waitAllUI;
      WaitUtil.ReleaseWaitWhile(waitAllUI);
      
      while (true) {
        // 更新UI
        if (currentUnit is Character) {
          EventCenter.Broadcast(EventType.ON_ENTER_PLAYER_TURN);
        } else if (s_lastUnit is Character) {
          EventCenter.Broadcast(EventType.ON_ENTER_FIRST_ENEMY_TURN);
        }
        // 补充能量
        int energy = currentUnit.GetAttrib(AttribType.ENERGY);
        int maxEnergy = currentUnit.GetAttrib(AttribType.ENERGY, true);
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
          s_currentUnitIndex = (s_currentUnitIndex + 1) % units.Count;
        } while (currentUnit.isDead && s_lastUnit != currentUnit /*避免死循环*/);
        // 等待一会再进入下一个回合
        yield return new WaitForSeconds(LATE_TURN_WAITING_TIME);
      }
    }
  }
}