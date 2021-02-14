using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yume.Data;
using Yume.Resource;
using Object = UnityEngine.Object;

namespace Yume.Battle {
  public abstract class Unit {
    public int level { get; }
    public string id { get; }
    public string tag { get; }
    public string name { get; }
    private Attrib[] attribs { get; }
    public Animator animator { get; }
    public GameObject gameObject { get; }
    public int passiveSkillLevel { get; }
    public PassiveSkillPhase[] passiveSkillPhases { get; }
    public Blackboard blackboard { get; } = new Blackboard();
    protected Queue<(Card card, Unit target)> playCardQueue = new Queue<(Card, Unit)>(); // 出牌队列
    public bool isAlive => GetAttribValue(AttribType.HP) > 0;

    protected Unit(UnitData unit, Transform unitFolder) {
      var unitTemplate = ResourceManager.Load<UnitTemplate>(unit.id);
      id = unit.id;
      level = unit.level;
      tag = unitTemplate.tag;
      name = unitTemplate.name;
      passiveSkillLevel = unit.passiveSkillLevel;
      passiveSkillPhases = unitTemplate.passiveSkillPhases;
      attribs = MathUtil.CalculateAttribs(unit.id, unit.level);
      var prefab = ResourceManager.Load<GameObject>(unitTemplate.prefabName);
      gameObject = Object.Instantiate(prefab, unitFolder);
      animator = gameObject.GetComponent<Animator>();
    }

    public abstract IEnumerator OnTrick();

    public abstract void OnDie();

    public abstract void CleanUp();

    public float AddAttribValue(AttribType type, float value) {
      float result = attribs[(int)type].AddValue(value);
      OnAttribValueChanged(type, result);
      return result;
    }

    public float AddAttribMaxValue(AttribType type, float value) {
      float result = attribs[(int)type].AddMaxValue(value);
      OnAttribMaxValueChanged(type, result);
      return result;
    }

    public float GetAttribValue(AttribType type) {
      return attribs[(int)type].value;
    }

    public float GetAttribMaxValue(AttribType type) {
      return attribs[(int)type].maxValue;
    }

    protected abstract void OnAttribValueChanged(AttribType type, float delta);

    protected abstract void OnAttribMaxValueChanged(AttribType type, float delta);

    public void EnqueueCard((Card card, Unit target) cardTuple) => playCardQueue.Enqueue(cardTuple);
  }
}