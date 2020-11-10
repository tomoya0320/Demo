using System.Collections.Generic;

namespace Yume.FSM {
  public abstract class State {
    public int stateId { get; private set; }
    public StateMachine stateMachine { get; set; }

    public State(int stateId) {
      this.stateId = stateId;
    }

    public abstract void OnEnter();
    public abstract void OnUpdate();
    public abstract void OnFixedUpdate();
    public abstract void OnExit();
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

    public void OnUpdate() {
      if (currentState != null) {
        currentState.OnUpdate();
      }
    }

    public void OnFixedUpdate() {
      if (currentState != null) {
        currentState.OnFixedUpdate();
      }
    }

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