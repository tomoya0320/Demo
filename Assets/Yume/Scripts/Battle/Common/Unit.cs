using System;
using System.Collections.Generic;
using System.ComponentModel;
using Yume.Data;
using Yume.Resource;

namespace Yume.Battle {
  /// <summary>
  /// Description用来存储属性的实际显示名称
  /// </summary>
  public enum AttribType {
    [Description("生命")]
    HP,
    [Description("攻击")]
    ATK,
    [Description("防御")]
    DEF,
    [Description("命中")]
    HIT_PROB,
    [Description("回避")]
    EVADE_PROB,
    [Description("能量")]
    ENERGY,
  }

  [Serializable]
  public struct Attrib {
    public float value;
    public float maxValue;

    public static Attrib operator +(Attrib a1, Attrib a2) {
      Attrib attrib = new Attrib {
        value = a1.value + a2.value,
        maxValue = a1.maxValue + a2.maxValue
      };
      return attrib;
    }    
    
    public static Attrib operator -(Attrib a1, Attrib a2) {
      Attrib attrib = new Attrib {
        value = a1.value - a2.value,
        maxValue = a1.maxValue - a2.maxValue
      };
      return attrib;
    }

    public static Attrib operator *(Attrib a, float value) {
      Attrib attrib = new Attrib {
        value = value * a.value,
        maxValue = value * a.maxValue
      };
      return attrib;
    }

    public override string ToString() {
      return $"value:{value},maxValue:{maxValue}";
    }
  }

  public abstract class Unit {
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

    public enum CardHeapType {
      /// <summary>
      /// 抽牌堆
      /// </summary>
      DRAW,
      /// <summary>
      /// 弃牌堆
      /// </summary>
      DISCARD,
      /// <summary>
      /// 耗牌堆
      /// </summary>
      CONSUME,
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

    protected int level;
    protected string id;
    protected string tag;
    protected string name;
    protected Attrib[] attribs;
    protected StateMachine stateMachine;
    protected Dictionary<CardHeapType,List<CardData>> cardHeapDict; //牌堆(抽牌、弃牌、耗牌)

    public bool isAlive => attribs[(int)AttribType.HP].value > 0;

    public Unit(UnitData unit, List<CardData> cardList) {
      UnitTemplate unitTemplate = ResourceManager.Load<UnitTemplate>(unit.id);
      id = unit.id;
      level = unit.level;
      tag = unitTemplate.tag;
      name = unitTemplate.name;
      attribs = MathUtil.CalculateAttribs(unit.id, unit.level, unitTemplate.maxLevel,
                                          unitTemplate.GetMinAttribs(), unitTemplate.GetMaxAttribs());
      //TODO:Init PassiveSkill
      //TODO:Init Prefab

      cardHeapDict = new Dictionary<CardHeapType, List<CardData>>();
      foreach (CardHeapType cardHeapType in Enum.GetValues(typeof(CardHeapType))) {
        cardHeapDict.Add(cardHeapType, new List<CardData>());
      }
      cardHeapDict[CardHeapType.DRAW].AddRange(cardList);

      stateMachine = new StateMachine(new IdleState(this));
      stateMachine.RegisterState(new PlayState(this));
      stateMachine.RegisterState(new DeadState(this));
    }

    public virtual void SwitchState(State state, bool force = false) {
      stateMachine.SwitchState((int)state, force);
    }
  }
}