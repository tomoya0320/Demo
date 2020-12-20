using System.Collections.Generic;

namespace Yume.FSM {
  public abstract class State {
    public int stateId { get; private set; }
    public StateMachine stateMachine { get; set; }

    public State(int stateId) {
      this.stateId = stateId;
    }

    public virtual void OnEnter() { }
    public virtual void OnTrick() { }
    public virtual void OnExit() { }
  }

  public abstract class StateBase<T> : State {
    public T owner { get; private set; }

    public StateBase(int id, T owner) : base(id) {
      this.owner = owner;
    }
  }

  public class StateMachine {
    private Dictionary<int, State> m_states = new Dictionary<int, State>();
    public State currentState { get; private set; } = null;
    public State lastState { get; private set; } = null;

    public StateMachine(State initState) {
      if (RegisterState(initState)) {
        currentState = initState;
        currentState.OnEnter();
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
      if (currentState != null) {
        currentState.OnTrick();
      }
    }

    /// <summary>
    /// 切换状态
    /// </summary>
    /// <param name="nextStateId">下一个状态的ID</param>
    /// <param name="force">只有当前状态和下一个状态不一致时才会切换，当force为true时忽略这条规则</param>
    /// <returns></returns>
    public bool SwitchState(int nextStateId, bool force = false) {
      if (m_states.ContainsKey(nextStateId) && (force ||
          currentState.stateId != nextStateId)) {

        lastState = currentState;
        currentState = m_states[nextStateId];
        currentState.OnEnter();
        return true;
      }
      return false;
    }
  }
}