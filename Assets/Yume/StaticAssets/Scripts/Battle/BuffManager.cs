using System.Collections.Generic;
using UnityEngine;

namespace Yume.Battle {
  public class BuffManager {
    public Unit unit { get; }
    public Dictionary<string, List<Buff>> buffDict => m_buffDict;
    private Dictionary<string, List<Buff>> m_buffDict = new Dictionary<string, List<Buff>>();

    public BuffManager(Unit unit) {
      this.unit = unit;
    }

    public void AddBuff(string buffId, StackType stackType, Blackboard blackboard, Behavior[] behaviors, BuffDisplay display) {
      if (!m_buffDict.ContainsKey(buffId)) {
        m_buffDict.Add(buffId, new List<Buff>());
      }
      switch (stackType) {
        case StackType.NONE:
          if (m_buffDict[buffId].Count == 0) {
            _AddBuffInternal(buffId, blackboard, behaviors, display);
          }
          break;
        case StackType.DEFAULT:
          _AddBuffInternal(buffId, blackboard, behaviors, display);
          break;
      }
    }

    public bool RemoveBuff(Buff buff) {
      if (m_buffDict.ContainsKey(buff.id) && m_buffDict[buff.id].Remove(buff)) {
        buff.OnRemove();
        return true;
      }
      Debug.LogError($"移除buff出错！buff:{buff.id}不存在！");
      return false;
    }

    private void _AddBuffInternal(string buffId, Blackboard blackboard, Behavior[] behaviors, BuffDisplay display) {
      Buff buff = new Buff(unit, buffId, blackboard, behaviors, display);
      if (buff.duration == 0) {
        // 持续回合为0表示瞬发
        // 不会走Add和Remove流程
        if (buff.behaviorTreeDict.TryGetValue(BehaviorTime.ON_INSTANT, out var behaviorTrees)) {
          behaviorTrees.ForEach(behaviorTree => behaviorTree.Run());
        }
      } else {
        buff.OnAdd();
        m_buffDict[buffId].Add(buff);
      }
    }

    public void OnLateUpdate() {
      foreach (var buffs in m_buffDict.Values) {
        for (int i = buffs.Count - 1; i >= 0; i--) {
          if (!buffs[i].OnUpdate()) {
            buffs[i].OnRemove();
            buffs.RemoveAt(i);
          }
        }
      }
    }

    public void OnCasterDie(Unit caster) {
      foreach (var buffs in m_buffDict.Values) {
        for (int i = buffs.Count - 1; i >= 0; i--) {
          if (buffs[i].blackboard.GetValue<Unit>(Blackboard.CASTER) == caster) {
            buffs[i].OnRemove();
            buffs.RemoveAt(i);
          }
        }
      }
    }

    public bool ContainBuff(string buffId) {
      return m_buffDict.TryGetValue(buffId, out var buffs) && buffs.Count > 0;
    }

    /// <summary>
    /// 目前只支持回合结束后的行为
    /// </summary>
    /// <param name="buffId"></param>
    public void TriggerBuff(string buffId) {
      if (m_buffDict.TryGetValue(buffId, out var buffs) && buffs.Count > 0) {
        foreach (var buff in buffs) {
          if (buff.behaviorTreeDict.TryGetValue(BehaviorTime.ON_LATE_TURN, out var behaviorTrees)) {
            for (int i = buff.turn; i < buff.duration; i++) {
              foreach (var behaviorTree in behaviorTrees) {
                behaviorTree.Run();
                if (!unit.isAlive) {
                  break;
                }
              }
              if (!unit.isAlive) {
                break;
              }
            }
            buff.behaviorTreeDict.Remove(BehaviorTime.ON_LATE_TURN);
            var behaviorDict = BattleManager.behaviorManagers[unit].behaviorDict;
            foreach (var behaviorTree in behaviorTrees) {
              behaviorDict[BehaviorTime.ON_LATE_TURN].Remove(behaviorTree);
            }
          }
        }

        for (int i = buffs.Count - 1; i >= 0; i--) {
          if (buffs[i].behaviorTreeDict.Count == 0) {
            buffs[i].OnRemove();
            buffs.RemoveAt(i);
          }
        }
      }
    }
  }
}