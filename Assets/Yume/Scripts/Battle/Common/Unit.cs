using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
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
    private StateMachine m_stateMachine;
    private BuffContainer m_buffContainer;

    private Attribute m_attribute;
    /// <summary>
    /// 抽牌堆
    /// </summary>
    private List<CardDB> m_cardList;
    /// <summary>
    /// 弃牌堆
    /// </summary>
    private List<CardDB> m_discardCardList;
    /// <summary>
    /// 耗牌堆
    /// </summary>
    private List<CardDB> m_consumedCardList;

    public virtual void OnInit(Attribute attribute, List<CardDB> cardList, BuffData[] passiveBuffs = null) {
      m_animator = GetComponent<Animator>();

      m_stateMachine = new StateMachine(new IdleState(this));
      m_stateMachine.RegisterState(new PlayState(this));
      m_stateMachine.RegisterState(new DeadState(this));

      m_buffContainer = new BuffContainer(this);

      m_discardCardList = new List<CardDB>();
      m_consumedCardList = new List<CardDB>();

      m_attribute = attribute;

      m_cardList = cardList;

      if (passiveBuffs != null) {
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