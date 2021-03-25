using System.Collections.Generic;

namespace Yume.Battle {
  public abstract class State {
    public int stateId { get; private set; }
    public StateMachine stateMachine { get; set; }

    protected State(int stateId) {
      this.stateId = stateId;
    }

    public virtual void OnEnter(int lastStateId) { }
    public virtual void OnTrick() { }
    public virtual void OnExit(int nextStateId) { }
  }

  public abstract class StateBase<T> : State {
    public T owner { get; private set; }

    protected StateBase(int id, T owner) : base(id) {
      this.owner = owner;
    }
  }

  /// <summary>
  /// 默认的状态id为-1 占位用
  /// </summary>
  public class DefaultState : State {
    public DefaultState() : base(-1) { }
    
    public override void OnEnter(int lastStateId) { }

    public override void OnTrick() { }

    public override void OnExit(int nextStateId) { }
  }

  public abstract class StateMachine {
    private Dictionary<int, State> m_states = new Dictionary<int, State>();
    public State currentState { get; private set; }

    protected StateMachine() {
      State defaultState = new DefaultState();
      if (RegisterState(defaultState)) {
        currentState = defaultState;
      }
    }

    public bool RegisterState(State state) {
      if (state == null || m_states.ContainsKey(state.stateId)) {
        return false;
      }
      m_states.Add(state.stateId, state);
      state.stateMachine = this;
      
      return true;
    }

    public bool RemoveState(int stateId) {
      if (m_states.ContainsKey(stateId)) {
        m_states[stateId].stateMachine = null;
        m_states.Remove(stateId);
        
        return true;
      }
      return false;
    }

    public void OnTrick() {
      currentState?.OnTrick();
    }

    /// <summary>
    /// 切换状态
    /// </summary>
    /// <param name="nextStateId">下一个状态的ID</param>
    /// <param name="force">只有当前状态和下一个状态不一致时才会切换，当force为true时忽略这条规则</param>
    /// <returns></returns>
    public bool SwitchState(int nextStateId, bool force = false) {
      if (m_states.ContainsKey(nextStateId) && (currentState.stateId != nextStateId || force)) {
        int lastStateId = currentState.stateId;
        currentState.OnExit(nextStateId);
        currentState = m_states[nextStateId];
        currentState.OnEnter(lastStateId);
        
        return true;
      }
      return false;
    }
  }
}