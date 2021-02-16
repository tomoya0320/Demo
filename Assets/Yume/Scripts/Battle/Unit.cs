using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Yume.Data;
using Yume.Resource;
using Object = UnityEngine.Object;

namespace Yume.Battle {
  public abstract class Unit {
    protected Text hpText;
    protected Text energyText;
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
    public bool isAlive => GetAttrib(AttribType.HP) > 0;

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
      
      // 初始化UI
      var objectContainer = gameObject.GetComponent<ObjectContainer>();
      hpText = objectContainer.GetComponent<Text>("hp");
      energyText = objectContainer.GetComponent<Text>("energy");

      hpText.text = $"HP:{GetAttrib(AttribType.HP):0}/{GetAttrib(AttribType.HP, true):0}";
      energyText.text = $"E:{GetAttrib(AttribType.ENERGY):0}/{GetAttrib(AttribType.ENERGY, true):0}";
    }

    public abstract IEnumerator OnTrick();

    public abstract void OnDie();

    public abstract void CleanUp();

    public float AddAttrib(AttribType type, float value, bool onMaxValue = false) {
      float result;
      if (onMaxValue) {
        result = attribs[(int)type].AddMaxValue(value);
      } else {
        result = attribs[(int)type].AddValue(value);
      }
      OnAttribChanged(type, result, onMaxValue);
      return result;
    }

    public float GetAttrib(AttribType type, bool onMaxValue = false) {
      return onMaxValue ? attribs[(int)type].maxValue : attribs[(int)type].value;
    }
    
    protected virtual void OnAttribChanged(AttribType type, float delta, bool onMaxValue = false) {
      switch (type) {
        case AttribType.HP: {
          // 更新UI
          hpText.text = $"HP:{GetAttrib(AttribType.HP):0}/{GetAttrib(AttribType.HP, true):0}";
          break;
        }
        case AttribType.ATK:
          break;
        case AttribType.ENERGY:
          // 更新UI
          energyText.text = $"E:{GetAttrib(AttribType.ENERGY):0}/{GetAttrib(AttribType.ENERGY, true):0}";
          break;
        case AttribType.DEF:
          break;
      }
    }

    public void EnqueueCard((Card card, Unit target) cardTuple) => playCardQueue.Enqueue(cardTuple);
  }
}