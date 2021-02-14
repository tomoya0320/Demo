using System.Collections.Generic;
using UnityEngine;
using Yume.Data;

namespace Yume.Battle {
  public class BehaviorManager {
    public Unit unit { get; }
    private Dictionary<BehaviorTime, List<BehaviorTree>> m_behaviorDict = new Dictionary<BehaviorTime, List<BehaviorTree>>();
 
    public BehaviorManager(Unit unit) {
      this.unit = unit;
    }

    public void AddBehaviorTrees(BehaviorTime behaviorTime, params BehaviorTree[] behaviorTrees) {
      if (!m_behaviorDict.TryGetValue(behaviorTime, out var behaviorTreeList)) {
        behaviorTreeList = new List<BehaviorTree>();
        m_behaviorDict.Add(behaviorTime, behaviorTreeList);
      }
      behaviorTreeList.AddRange(behaviorTrees);
      if (behaviorTime == BehaviorTime.ON_ADD) {
        foreach (var behaviorTree in behaviorTrees) {
          behaviorTree.Run();
        }
      }
    }

    public void RemoveBehaviorTrees(BehaviorTime behaviorTime, params BehaviorTree[] behaviorTrees) {
      if (!m_behaviorDict.ContainsKey(behaviorTime)) {
        Debug.LogError($"未加载的BehaviorTime:{behaviorTime}！");
        return;
      }
      foreach (var behaviorTree in behaviorTrees) {
        m_behaviorDict[behaviorTime].Remove(behaviorTree);
        if (behaviorTime == BehaviorTime.ON_REMOVE) {
          behaviorTree.Run();
        }
      }
    }

    public void Run(BehaviorTime behaviorTime) {
      if (m_behaviorDict.ContainsKey(behaviorTime)) {
        m_behaviorDict[behaviorTime].ForEach(behaviorTree => behaviorTree.Run());
      }
    }
  }
}