using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Yume.Data;
using Yume.Resource;
using Yume.UI;

namespace Yume.Battle {
  public abstract class Unit {
    public int level { get; }
    public Camp camp { get; }
    public string id { get; }
    public string name { get; }
    private Attrib[] attribs { get; }
    public int passiveSkillLevel { get; }
    public PassiveSkillPhase[] passiveSkillPhases { get; }
    public Blackboard blackboard { get; } = new Blackboard();
    protected Queue<Card> playCardQueue = new Queue<Card>(); // 出牌队列
    public bool isAlive => GetAttrib(AttribType.HP) > 0;

    #region 表现层

    protected YImage hpImage;
    protected Tween hpImgTween;
    protected YImage energyImage;

    public Animator animator { get; }
    public GameObject selected { get; }
    public GameObject gameObject { get; }
    public Transform numReference { get; }
    protected abstract Transform folder { get; }

    #endregion

    protected Unit(UnitData unit) {
      var unitTemplate = ResourceManager.Load<UnitTemplate>(unit.id);
      level = unit.lv;
      id = unitTemplate.id;
      name = unitTemplate.name;
      camp = unitTemplate.camp;
      passiveSkillLevel = unit.passiveSkillLv;
      passiveSkillPhases = unitTemplate.passiveSkillPhases;
      attribs = MathUtil.CalculateAttribs(unit.id, unit.lv);

      // 表现层
      var prefab = ResourceManager.Load<GameObject>(unitTemplate.prefabName);
      gameObject = Object.Instantiate(prefab, folder);
      animator = gameObject.GetComponent<Animator>();

      var objectContainer = gameObject.GetComponent<ObjectContainer>();
      hpImage = objectContainer.GetComponent<YImage>("hp");
      energyImage = objectContainer.GetComponent<YImage>("energy");
      numReference = objectContainer.GetTransform("numReference");
      selected = objectContainer.GetGameObject("selected");
      selected?.SetActiveEx(false);

      // 更新属性UI
      OnAttribChanged(AttribType.HP);
      OnAttribChanged(AttribType.ENERGY);
    }

    public abstract IEnumerator OnTick();

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

    protected virtual void OnAttribChanged(AttribType type, float delta = 0, bool onMaxValue = false) {
      switch (type) {
        case AttribType.HP: {
          // 更新UI
          hpImgTween?.Complete();
          float fillAmount = GetAttrib(AttribType.HP) / GetAttrib(AttribType.HP, true);
          hpImgTween = hpImage.DOFillAmount(fillAmount, 0.2f).OnComplete(() => hpImgTween = null);
          hpImage.textComponent.text = $"{(int)GetAttrib(AttribType.HP)}/{(int)GetAttrib(AttribType.HP, true)}";
          break;
        }
        case AttribType.ENERGY:
          // 更新UI
          energyImage.textComponent.text = $"{(int)GetAttrib(AttribType.ENERGY)}/{(int)GetAttrib(AttribType.ENERGY, true)}";
          break;
      }
    }

    public void EnqueueCard(Card card) => playCardQueue.Enqueue(card);
  }
}