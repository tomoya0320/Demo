using System;
using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;
using UnityEngine;
using Yume.DB;
using Yume.FSM;
using Yume.Math;

namespace Yume.Battle {
  /// <summary>
  /// Description用来存储属性的实际显示名称
  /// </summary>
  public enum AttribType {
    [Description("生命")]
    HP,
    [Description("攻击力")]
    ATK,
    [Description("防御力")]
    DEF,
    [Description("命中率")]
    HIT_PROB,
    [Description("回避率")]
    EVADE_PROB,
    [Description("能量")]
    ENERGY,

    LENGTH // 属性类型总数
  }

  [Serializable]
  public struct Attrib {
    [JsonProperty]
    public float value { get; private set; }
    [JsonProperty]
    public float maxValue { get; private set; }

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
  }

  [Serializable]
  public class UnitDB {
    public string id;
    public int level;
    public int passiveSkillLevels;
  }

  [Serializable]
  public struct UnitInfo {
    [Serializable]
    public struct PassiveSkillPhase {
      public string description;
      public BuffData[] buffData;
      public Blackboard blackboard;
    }

    public string id;
    public string tag;
    public string name;
    public int maxLevel;
    public string prefabName;
    public PassiveSkillPhase[] passiveSkillPhases;
    public Dictionary<AttribType, Attrib> minAttribDict;
    public Dictionary<AttribType, Attrib> maxAttribDict;

    public Attrib[] GetMinAttribs() {
      Attrib[] minAttribs = new Attrib[(int)AttribType.LENGTH];
      foreach (var kv in minAttribDict) {
        minAttribs[(int)kv.Key] = kv.Value;
      }
      return minAttribs;
    }    
    
    public Attrib[] GetMaxAttribs() {
      Attrib[] maxAttribs = new Attrib[(int)AttribType.LENGTH];
      foreach (var kv in maxAttribDict) {
        maxAttribs[(int)kv.Key] = kv.Value;
      }
      return maxAttribs;
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

    protected int unitLevel;
    protected string unitId;
    protected string unitTag;
    protected string unitName;
    protected Attrib[] attribs;
    protected List<CardDB> cardList; //抽牌堆
    protected StateMachine stateMachine;
    protected BuffContainer buffContainer;
    protected List<CardDB> discardCardList = new List<CardDB>(); //弃堆
    protected List<CardDB> consumedCardList = new List<CardDB>(); //耗牌堆

    public virtual void OnInit(UnitDB unitDB, List<CardDB> cardList) {
      UnitInfo unitInfo = DBManager.instance.GetUnitInfo(unitDB.id);
      unitId = unitDB.id;
      unitTag = unitInfo.tag;
      unitLevel = unitDB.level;
      unitName = unitInfo.name;
      attribs = MathUtil.CalculateAttribs(unitDB.level, unitInfo.maxLevel,
                         unitInfo.GetMinAttribs(), unitInfo.GetMaxAttribs());
      //TODO:Init PassiveSkill
      //TODO:Init Prefab
      this.cardList = cardList;

      buffContainer = new BuffContainer(this);
      stateMachine = new StateMachine(new IdleState(this));

      stateMachine.RegisterState(new PlayState(this));
      stateMachine.RegisterState(new DeadState(this));
    }

    public virtual void SwitchState(State state, bool force = false) {
      stateMachine.SwitchState((int)state, force);
    }
  }
}