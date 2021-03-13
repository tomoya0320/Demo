using System.Collections.Generic;
using UnityEngine;
using Yume.Data;
using Yume.Resource;

namespace Yume.Battle {
  public class Buff {
    public Unit unit { get; }
    public string id { get; }
    public int duration { get; }
    // buff下的行为树共用
    // 因为是引用类型所以很方便
    public Blackboard blackboard { get; }
    public int turn { private set; get; }
    // 这个buff所属的行为树
    private Dictionary<BehaviorTime, List<BehaviorTree>> m_behaviorTreeDict = new Dictionary<BehaviorTime, List<BehaviorTree>>();

    public Buff(Unit unit, string id, Blackboard blackboard, Behavior[] behaviors) {
      this.id = id;
      this.unit = unit;
      this.blackboard = blackboard;
      // 这些属性从blackboard中取
      duration = blackboard.GetValue<int>("duration");

      foreach (var behavior in behaviors) {
        if (!m_behaviorTreeDict.TryGetValue(behavior.behaviorTime, out var behaviorTrees)) {
          behaviorTrees = new List<BehaviorTree>();
          m_behaviorTreeDict.Add(behavior.behaviorTime, behaviorTrees);
        }
        // 初始化行为树
        BehaviorTree behaviorTree = ResourceManager.Load<BehaviorTree>(behavior.behaviorTreeId).Copy() as BehaviorTree;
        behaviorTree.unit = unit;
        behaviorTree.buff = this;
        behaviorTree.blackboard = blackboard;
        behaviorTrees.Add(behaviorTree);
      }
    }

    public void OnAdd() {
      // 这时候行为树才正式添加
      if (!BattleManager.behaviorManagers.ContainsKey(unit)) {
        Debug.LogError($"单位[{unit.id}]的BehaviorManager未正确初始化！");
        return;
      }
      foreach (var kv in m_behaviorTreeDict) {
        BehaviorTime behaviorTime = kv.Key;
        foreach (var behavior in kv.Value) {
          BattleManager.behaviorManagers[unit].AddBehaviorTree(behaviorTime, behavior);       
        }
      }
    }

    public void OnRemove() {
      // 同时移除行为树
      if (!BattleManager.behaviorManagers.ContainsKey(unit)) {
        Debug.LogError($"单位:{unit.name}的BehaviorManager未正确初始化！");
        return;
      }
      foreach (var kv in m_behaviorTreeDict) {
        BehaviorTime behaviorTime = kv.Key;
        foreach (var behavior in kv.Value) {
          BattleManager.behaviorManagers[unit].RemoveBehaviorTree(behaviorTime, behavior);       
        }
      }
    }

    public bool OnUpdate() {
      // 持续时间小于0表示无限直接返回true
      if (duration < 0) {
        return true;
      }
      
      turn++;
      return turn < duration;
    }
  }
}