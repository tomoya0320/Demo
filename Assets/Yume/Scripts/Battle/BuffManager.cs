using System.Collections.Generic;
using Yume.Data;

namespace Yume.Battle {
  public class BuffManager {
    public Unit unit { get; }
    private Dictionary<string, List<Buff>> m_buffDict = new Dictionary<string, List<Buff>>();

    public BuffManager(Unit unit) {
      this.unit = unit;
    }

    public void AddBuff(string buffId, StackType stackType, Blackboard blackboard, Behavior[] behaviors) {
      if (!m_buffDict.ContainsKey(buffId)) {
        m_buffDict.Add(buffId, new List<Buff>());
      }
      switch (stackType) {
        case StackType.NONE:
          if (m_buffDict[buffId].Count == 0) {
            _AddBuffInternal(buffId, blackboard, behaviors);
          }
          break;
        case StackType.DEFAULT:
          _AddBuffInternal(buffId, blackboard, behaviors);
          break;
      }
    }

    private void _AddBuffInternal(string buffId, Blackboard blackboard, Behavior[] behaviors) {
      Buff buff = new Buff(unit, buffId, blackboard, behaviors);
      // 持续回合为0表示瞬发
      buff.OnAdd();
      if (buff.duration == 0) {
        buff.OnRemove();
      } else {
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
  }
}