using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Yume.UI;
using Object = UnityEngine.Object;

namespace Yume.Battle {
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
    /// <summary>
    /// 手牌堆
    /// </summary>
    HAND,
    /// <summary>
    /// 出牌堆
    /// </summary>
    PLAY,
  }

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
    public bool isDead => !isAlive;
    protected Dictionary<CardHeapType, List<Card>> cardHeapDict = new Dictionary<CardHeapType, List<Card>>(); //牌堆(抽牌、弃牌、耗牌、手牌)

    #region 表现层

    protected YImage hpImg;
    protected Image hpTransitionImg;
    protected YImage energyImage;
    protected float hpTransitionSpeed => 0.6f * Time.deltaTime;

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
      // 初始化单位属性
      var unitTemplate = ResourceManager.Load<UnitTemplate>(unitArgs.id);
      level = unitArgs.lv;
      id = unitTemplate.id;
      name = unitTemplate.name;
      camp = unitTemplate.camp;
      passiveSkillLevel = unitArgs.passiveSkillLv;
      passiveSkillPhases = unitTemplate.passiveSkillPhases;
      attribs = MathUtil.CalculateAttribs(unitArgs.id, unitArgs.lv);

      // 表现层
      gameObject = ObjectUtil.Instantiate(unitTemplate.prefabName, folder);
      animator = gameObject.GetComponent<Animator>();

      var objectContainer = gameObject.GetComponent<ObjectContainer>();
      objectContainer.GetComponent<Button>("detailButton").onClick.AddListener(
        () => UIManager.OpenOne("battle_unit_detail", null, this)
      );
      hpImg = objectContainer.GetComponent<YImage>("hp");
      hpTransitionImg = objectContainer.GetComponent<Image>("hpTransition");
      energyImage = objectContainer.GetComponent<YImage>("energy");
      numReference = objectContainer.GetTransform("numReference");
      selected = objectContainer.GetGameObject("selected");
      selected?.SetActiveEx(false);

      // 初始化卡牌堆字典
      foreach (CardHeapType cardHeapType in Enum.GetValues(typeof(CardHeapType))) {
        cardHeapDict.Add(cardHeapType, new List<Card>());
      }

      // 更新属性UI
      OnAttribChanged(AttribType.HP);
      OnAttribChanged(AttribType.ENERGY);
    }

    public abstract IEnumerator OnTick();

    public virtual void CleanUp() {
      blackboard.Clear();

      foreach (var cards in cardHeapDict.Values) {
        foreach (var card in cards) {
          card.CleanUp();
        }
        cards.Clear();
      }
      cardHeapDict.Clear();

      Object.Destroy(gameObject);
    }

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
          if (hpImg != null) {
            hpImg.fillAmount = (float)GetAttrib(AttribType.HP) / GetAttrib(AttribType.HP, true);
            hpImg.textComponent.text = $"{GetAttrib(AttribType.HP)}/{GetAttrib(AttribType.HP, true)}";
          }
          break;
        }
        case AttribType.ENERGY:
          if (energyImage != null) {
            energyImage.textComponent.text = $"{GetAttrib(AttribType.ENERGY)}/{GetAttrib(AttribType.ENERGY, true)}";
          }
          break;
      }
    }

    public void EnqueueCard(Card card) {
      cardHeapDict[CardHeapType.PLAY].Add(card);
      if (card != null) {
        card.cardHeapType = CardHeapType.PLAY;
        // 注意结束回合的标志是一个空的卡牌这个不能统计进去
        blackboard[Blackboard.PLAY_CARD_COUNT] = blackboard.GetValue<int>(Blackboard.PLAY_CARD_COUNT) + 1;
      }
    }

    public virtual void OnUpdate() {
      if (isDead) {
        return;
      }

      // 更新血条UI
      if (hpImg != null && hpTransitionImg != null) {
        if (hpTransitionImg.fillAmount < hpImg.fillAmount) {
          hpTransitionImg.fillAmount = hpImg.fillAmount;
        } else {
          hpTransitionImg.fillAmount = Mathf.Max(hpImg.fillAmount, hpTransitionImg.fillAmount - hpTransitionSpeed);
        }
      }

      // 更新卡牌UI 
      foreach (var cards in cardHeapDict.Values) {
        cards.ForEach(card => card?.OnUpdate());
      }
    }
  }
}