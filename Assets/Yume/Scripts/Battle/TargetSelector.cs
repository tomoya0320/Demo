using System.Collections.Generic;
using UnityEngine;
using Yume.Data;

namespace Yume.Battle {
  public abstract class TargetSelector {
    public abstract List<Unit> Select(Unit caster, Unit candidate);
  }

  /// <summary>
  /// 从自己和选择的目标中选一个
  /// </summary>
  public class SingleSelector : TargetSelector {
    private enum TargetType {
      /// <summary>
      /// 自己
      /// </summary>
      CASTER,
      /// <summary>
      /// 选中的目标
      /// </summary>
      CANDIDATE,
    }

    [SerializeField]
    private TargetType _targetType;

    public override List<Unit> Select(Unit caster, Unit candidate) {
      List<Unit> targets = new List<Unit>();

      switch (_targetType) {
        case TargetType.CASTER:
          targets.Add(caster);
          break;
        case TargetType.CANDIDATE:
          targets.Add(candidate);
          break;
      }

      return targets;
    }
  }

  /// <summary>
  /// 通过阵营选择
  /// </summary>
  public class CampSelector : TargetSelector {
    [SerializeField]
    private Camp _camp;
    [SerializeField]
    private bool _includeSelf;

    public override List<Unit> Select(Unit caster, Unit candidate) {
      List<Unit> targets = new List<Unit>();
      foreach (var unit in BattleManager.units) {
        if ((unit.camp == _camp) && (unit != caster || _includeSelf)) {
          targets.Add(unit);
        }
      }
      return targets;
    }
  }
}