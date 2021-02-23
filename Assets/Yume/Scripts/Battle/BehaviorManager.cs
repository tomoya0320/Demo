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

    public void AddBehaviorTree(BehaviorTime behaviorTime, BehaviorTree behaviorTree) {
      if (!m_behaviorDict.TryGetValue(behaviorTime, out var behaviorTrees)) {
        behaviorTrees = new List<BehaviorTree>();
        m_behaviorDict.Add(behaviorTime, behaviorTrees);
      }
      behaviorTrees.Add(behaviorTree);
      if (behaviorTime == BehaviorTime.ON_ADD) {
        behaviorTree.Run();
      }
    }

    public void RemoveBehaviorTree(BehaviorTime behaviorTime, BehaviorTree behaviorTree) {
      if (!m_behaviorDict.ContainsKey(behaviorTime)) {
        Debug.LogError($"未加载的BehaviorTime:{behaviorTime}！");
        return;
      }
      if (m_behaviorDict[behaviorTime].Remove(behaviorTree)) {
        if (behaviorTime == BehaviorTime.ON_REMOVE) {
          behaviorTree.Run();
        }
      } else {
        Debug.LogError($"remove behaviorTree error.behaviorTree not exist");
      }
    }

    public void Run(BehaviorTime behaviorTime) {
      if (m_behaviorDict.ContainsKey(behaviorTime)) {
        m_behaviorDict[behaviorTime].ForEach(behaviorTree => behaviorTree.Run());
      }
    }
  }
}