using System.Collections.Generic;

namespace Yume.Battle {
  public abstract class State {
    public int stateId { get; private set; }
    public StateMachine stateMachine { get; set; }

    protected State(int stateId) {
      this.stateId = stateId;
    }

    public virtual void OnEnter(int lastStateId) { }
    public virtual void OnTick() { }
    public virtual void OnExit(int nextStateId) { }
  }

  public abstract class StateBase<T> : State {
    public T owner { get; private set; }

    protected StateBase(int id, T owner) : base(id) {
      this.owner = owner;
    }
  }

  public abstract class StateMachine {
    protected Dictionary<int, State> states = new Dictionary<int, State>();
    public State currentState { get; protected set; }

    public bool RegisterState(State state) {
      if (state == null || states.ContainsKey(state.stateId)) {
        return false;
      }
      states.Add(state.stateId, state);
      state.stateMachine = this;
      
      return true;
    }

    public bool RemoveState(int stateId) {
      if (states.ContainsKey(stateId)) {
        states[stateId].stateMachine = null;
        states.Remove(stateId);
        
        return true;
      }
      return false;
    }

    public void OnTick() {
      currentState?.OnTick();
    }

    /// <summary>
    /// 切换状态
    /// </summary>
    /// <param name="nextStateId">下一个状态的ID</param>
    /// <param name="force">只有当前状态和下一个状态不一致时才会切换，当force为true时忽略这条规则</param>
    /// <returns></returns>
    public bool SwitchState(int nextStateId, bool force = false) {
      if (states.ContainsKey(nextStateId) && (currentState.stateId != nextStateId || force)) {
        int lastStateId = currentState.stateId;
        currentState.OnExit(nextStateId);
        currentState = states[nextStateId];
        currentState.OnEnter(lastStateId);
        
        return true;
      }
      return false;
    }
  }
}