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
    public float atk;
    public float def;
    public float hitProb;
    public float evadeProb;
    public int energy;
    public int maxEnergy;
  }

  [RequireComponent(typeof(Animator), typeof(SpriteRenderer))]
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

    private Animator m_animator;
    private Attribute m_attribute;
    private StateMachine m_stateMachine;
    private BuffContainer m_buffContainer;

    public virtual void OnInit(JObject data) {
      m_animator = GetComponent<Animator>();

      m_stateMachine = new StateMachine(new IdleState(this));
      m_stateMachine.RegisterState(new PlayState(this));
      m_stateMachine.RegisterState(new DeadState(this));

      m_buffContainer = new BuffContainer(this);

      if (data == null) {
        throw new Exception("Unit data is null!");
      }

      if (data.ContainsKey(StringMap.ATTRIBUTE)) {
        m_attribute = JsonConvert.DeserializeObject<Attribute>(data[StringMap.ATTRIBUTE].ToString());
      } else {
        throw new Exception("Attribute data is null!");
      }

      if (data.ContainsKey(StringMap.PASSIVE_BUFFS)) {
        var passiveBuffs = JsonConvert.DeserializeObject<BuffData[]>(data[StringMap.PASSIVE_BUFFS].ToString());
        foreach (var buff in passiveBuffs) {
          m_buffContainer.AddBuff(buff);
        }
      }
    }

    public virtual void SwitchState(State state, bool force = false) {
      m_stateMachine.SwitchState((int)state, force);
    }
  }
}