using System;
using System.Collections.Generic;

namespace Yume.BehaviorTree {
  //状态枚举类型
  public enum BTNodeResult {
    FAILURE,
    SUCCESS,
    RUNNING,
  }

  public enum BTNodeType {
    CONDITION,
    ACTION,
    SELECTOR,
    SEQUENCE,
    PARALLEL,
    DECORATOR,
  }

  //一些初始化的静态方法
  public class BT {
    public static ConditionNode ConditionNode(Func<bool> conditionFunc) {
      return new ConditionNode(conditionFunc);
    }
    public static ActionNode ActionNode(Action immediateAction) {
      return new ActionNode(immediateAction);
    }
    public static ActionNode ActionNode(Func<IEnumerator<BTNodeResult>> continuousAction) {
      return new ActionNode(continuousAction);
    }
    public static SelectorNode SelectorNode() {
      return new SelectorNode();
    }
    public static SequenceNode SequenceNode() {
      return new SequenceNode();
    }
    public static ParallelNode ParallelNode() {
      return new ParallelNode();
    }
    public static InverseDecoratorNode InverseDecoratorNode(BaseNode childeNode) {
      return new InverseDecoratorNode(childeNode);
    }
  }

  /// <summary>
  /// 所有节点的基类
  /// </summary>
  public abstract class BaseNode {
    public BTNodeType nodeType;//节点类型
    public int nodeIndex; //在父节点的子节点列表里的索引号

    public BaseNode(BTNodeType nodeType) {
      this.nodeType = nodeType;
    }

    public abstract BTNodeResult Execute(); //执行方法
  }

  /// <summary>
  /// 组合节点类
  /// </summary>
  public abstract class CombinerNode : BaseNode {
    protected List<BaseNode> childNodeList = new List<BaseNode>();

    public CombinerNode(BTNodeType nodeType) : base(nodeType) { }

    public CombinerNode AddNode(params BaseNode[] nodes) {
      int startIndex = childNodeList.Count;
      for (int i = 0; i < nodes.Length; i++) {
        nodes[i].nodeIndex = startIndex + i;
        childNodeList.Add(nodes[i]);
      }
      return this;
    }
  }

  /// <summary>
  /// 条件节点
  /// </summary>
  public class ConditionNode : BaseNode {
    private Func<bool> m_conditionFunc;

    public ConditionNode(Func<bool> conditionFunc) : base(BTNodeType.CONDITION) {
      m_conditionFunc = conditionFunc;
    }

    public override BTNodeResult Execute() {
      return m_conditionFunc() ? BTNodeResult.SUCCESS : BTNodeResult.FAILURE;
    }
  }

  /// <summary>
  /// 行为节点
  /// </summary>
  public class ActionNode : BaseNode {
    private Action m_immediateAction; //瞬时行为
    private Func<IEnumerator<BTNodeResult>> m_continuousAction;//持续行为
    private IEnumerator<BTNodeResult> m_coroutine;

    public ActionNode(Action immediateAction) : base(BTNodeType.ACTION) {
      m_immediateAction = immediateAction;
    }

    public ActionNode(Func<IEnumerator<BTNodeResult>> continuousAction) : base(BTNodeType.ACTION) {
      m_continuousAction = continuousAction;
    }

    public override BTNodeResult Execute() {
      if (m_immediateAction != null) {
        m_immediateAction();
        return BTNodeResult.SUCCESS;
      }
      if (m_coroutine == null) {
        m_coroutine = m_continuousAction();
        return BTNodeResult.RUNNING;
      }
      m_coroutine.MoveNext();
      BTNodeResult result = m_coroutine.Current;
      if (result != BTNodeResult.RUNNING) {
        m_coroutine = null;
      }
      return result;
    }
  }

  /// <summary>
  /// 选择节点
  /// </summary>
  public class SelectorNode : CombinerNode {
    private BaseNode m_lastRunningNode;

    public SelectorNode() : base(BTNodeType.SELECTOR) { }

    public override BTNodeResult Execute() {
      int index = 0;
      if (m_lastRunningNode != null) {
        index = m_lastRunningNode.nodeIndex;
      }
      m_lastRunningNode = null;
      BTNodeResult resultType = BTNodeResult.FAILURE;
      for (int i = index; i < childNodeList.Count; i++) {
        BaseNode tempNode = childNodeList[i];
        resultType = tempNode.Execute();
        if (resultType == BTNodeResult.FAILURE) {
          continue;
        }
        if (resultType == BTNodeResult.SUCCESS) {
          break;
        }
        if (resultType == BTNodeResult.RUNNING) {
          m_lastRunningNode = tempNode;
          break;
        }
      }
      return resultType;
    }
  }

  /// <summary>
  /// 顺序节点
  /// </summary>
  public class SequenceNode : CombinerNode {
    private BaseNode m_lastRunningNode;

    public SequenceNode() : base(BTNodeType.SEQUENCE) { }

    public override BTNodeResult Execute() {
      int index = 0;
      if (m_lastRunningNode != null) {
        index = m_lastRunningNode.nodeIndex;
      }
      m_lastRunningNode = null;
      BTNodeResult resultType = BTNodeResult.FAILURE;
      for (int i = index; i < childNodeList.Count; i++) {
        BaseNode tempNode = childNodeList[i];
        resultType = tempNode.Execute();

        if (resultType == BTNodeResult.FAILURE) {
          break;
        }
        if (resultType == BTNodeResult.SUCCESS) {
          continue;
        }
        if (resultType == BTNodeResult.RUNNING) {
          m_lastRunningNode = tempNode;
          break;
        }
      }
      return resultType;
    }
  }

  /// <summary>
  /// 并行节点
  /// </summary>
  public class ParallelNode : CombinerNode {
    public ParallelNode() : base(BTNodeType.PARALLEL) { }

    public override BTNodeResult Execute() {
      int successCount = 0;
      int runningCount = 0;
      for (int i = 0; i < childNodeList.Count; i++) {
        BaseNode tempNode = childNodeList[i];
        BTNodeResult tempResult = tempNode.Execute();
        if (tempResult == BTNodeResult.SUCCESS) {
          successCount++;
        } else if (tempResult == BTNodeResult.RUNNING) {
          runningCount++;
        }
      }
      if (runningCount > 0) {
        return BTNodeResult.RUNNING;
      } else if (successCount > 0) {
        return BTNodeResult.SUCCESS;
      } else {
        return BTNodeResult.FAILURE;
      }
    }
  }

  /// <summary>
  /// 修饰节点(取反)
  /// </summary>
  public class InverseDecoratorNode : BaseNode {
    private BaseNode m_childNode;

    public InverseDecoratorNode(BaseNode childNode) : base(BTNodeType.DECORATOR) {
      m_childNode = childNode;
    }

    public override BTNodeResult Execute() {
      BTNodeResult tempResult = m_childNode.Execute();
      if (tempResult == BTNodeResult.FAILURE) {
        return BTNodeResult.SUCCESS;
      } else {
        return BTNodeResult.FAILURE;
      }
    }
  }
}