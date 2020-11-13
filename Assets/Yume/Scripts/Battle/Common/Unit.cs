using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using UnityEngine;
using Yume.FSM;
using Yume.String;

namespace Yume.Battle {
  [Serializable]
  public struct Attribute {
    public string tag;
    public float hp;
    public float maxHp;
    public float energy;
    public float maxEnergy;
    public float def;
    public float evadeProb;
  }

  [RequireComponent(typeof(Animator),typeof(SpriteRenderer))]
  public abstract class Unit : MonoBehaviour {
    public enum State {
      /// <summary>
      /// 回合外等待状态
      /// </summary>
      IDLE,
      /// <summary>
      /// 回合中出牌阶段
      /// </summary>
      PLAY,
      /// <summary>
      /// 死亡阶段
      /// </summary>
      DEAD
    }

    public class IdleState : StateBase<Unit> {
      public IdleState(Unit onwer) : base((int)State.IDLE, onwer) { }

      public override void OnEnter() {

      }

      public override void OnExit() {

      }
    }

    public class PlayState : StateBase<Unit> {
      //TODO:出牌的队列OnTrick
      public PlayState(Unit onwer) : base((int)State.PLAY, onwer) { }

      public override void OnEnter() {

      }

      public override void OnTrick() {

      }

      public override void OnExit() {

      }
    }

    public class DeadState : StateBase<Unit> {
      public DeadState(Unit onwer) : base((int)State.DEAD, onwer) { }

      public override void OnEnter() {

      }

      public override void OnExit() {

      }
    }

    #region Unit Data
    [SerializeField]
    private Attribute _attribute;
    [SerializeField]
    private BuffData[] _passiveBuffs;
    #endregion
    public Animator animator { get; private set; }
    public StateMachine stateMachine { get; private set; }
    public BuffContainer buffContainer { get; private set; }

    public virtual void OnInit(JObject data) {
      if (data != null) {
        if (data.ContainsKey(StringMap.ATTRIBUTE)) {
          _attribute = JsonConvert.DeserializeObject<Attribute>(data[StringMap.ATTRIBUTE].ToString());
        }
        if (data.ContainsKey(StringMap.PASSIVE_BUFFS)) {
          _passiveBuffs = JsonConvert.DeserializeObject<BuffData[]>(data[StringMap.PASSIVE_BUFFS].ToString());
        }
      }

      animator = GetComponent<Animator>();

      stateMachine = new StateMachine(new IdleState(this));
      stateMachine.RegisterState(new PlayState(this));
      stateMachine.RegisterState(new DeadState(this));

      buffContainer = new BuffContainer();
      foreach(var buffData in _passiveBuffs) {
        buffContainer.AddBuff(buffData);
      }
    }

    public virtual void SwitchState(State state,bool force = false) {
      stateMachine.SwitchState((int)state, force);
    }
  }
}