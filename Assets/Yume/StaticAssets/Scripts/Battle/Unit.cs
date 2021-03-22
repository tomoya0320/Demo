using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Yume.UI;

namespace Yume.Battle {
  public abstract class Unit {
    public int level { get; }
    public Camp camp { get; }
    public string id { get; }
    public string name { get; }
    public Attrib[] attribs { get; }
    public int passiveSkillLevel { get; }
    public PassiveSkillPhase[] passiveSkillPhases { get; }
    public Card playingCard; // 正在出的牌
    public Blackboard blackboard = new Blackboard();
    public bool isAlive => GetAttrib(AttribType.HP) > 0;
    protected Queue<Card> playCardQueue = new Queue<Card>(); // 出牌队列
    
    #region 表现层

    protected YImage hpImage;
    protected Tween hpImgTween;
    protected YImage energyImage;

    private int m_selectedEnable;
    public Animator animator { get; }
    public GameObject gameObject { get; }
    protected GameObject selected { get; }
    public Transform numReference { get; }
    protected abstract Transform folder { get; }

    public void SetSelectedEnable(bool enable) {
      m_selectedEnable += enable ? 1 : -1;
      selected.SetActiveEx(m_selectedEnable > 0);
    }

    #endregion

    protected Unit(UnitArgs unitArgs) {
      var unitTemplate = ResourceManager.Load<UnitTemplate>(unitArgs.id);
      level = unitArgs.lv;
      id = unitTemplate.id;
      name = unitTemplate.name;
      camp = unitTemplate.camp;
      passiveSkillLevel = unitArgs.passiveSkillLv;
      passiveSkillPhases = unitTemplate.passiveSkillPhases;
      attribs = MathUtil.CalculateAttribs(unitArgs.id, unitArgs.lv);

      // 表现层
      var prefab = ResourceManager.Load<GameObject>(unitTemplate.prefabName);
      gameObject = Object.Instantiate(prefab, folder);
      animator = gameObject.GetComponent<Animator>();

      var objectContainer = gameObject.GetComponent<ObjectContainer>();
      objectContainer.GetComponent<Button>("detailButton").onClick.AddListener(
        () =>
          UIManager.OpenOne("battle_unit_detail", null, this)
      );
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

    public int AddAttrib(AttribType type, int value, bool onMaxValue = false) {
      int result;
      if (onMaxValue) {
        result = attribs[(int)type].AddMaxValue(value);
      } else {
        result = attribs[(int)type].AddValue(value);
      }
      OnAttribChanged(type, result, onMaxValue);
      return result;
    }

    public int GetAttrib(AttribType type, bool onMaxValue = false) {
      return onMaxValue ? attribs[(int)type].maxValue : attribs[(int)type].value;
    }

    protected virtual void OnAttribChanged(AttribType type, int delta = 0, bool onMaxValue = false) {
      switch (type) {
        case AttribType.HP: {
          // 更新UI
          hpImgTween?.Complete();
          float fillAmount = (float)GetAttrib(AttribType.HP) / GetAttrib(AttribType.HP, true);
          hpImgTween = hpImage.DOFillAmount(fillAmount, 0.2f).OnComplete(() => hpImgTween = null);
          hpImage.textComponent.text = $"{GetAttrib(AttribType.HP)}/{GetAttrib(AttribType.HP, true)}";
          break;
        }
        case AttribType.ENERGY:
          // 更新UI
          energyImage.textComponent.text = $"{GetAttrib(AttribType.ENERGY)}/{GetAttrib(AttribType.ENERGY, true)}";
          break;
      }
    }

    public void EnqueueCard(Card card) => playCardQueue.Enqueue(card);
  }
}