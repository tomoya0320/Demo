using System.Collections;
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
    public bool isDead => !isAlive;
    protected YWaitUntil waitPlayCard;

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

      // 更新属性UI
      OnAttribChanged(AttribType.HP);
      OnAttribChanged(AttribType.ENERGY);
    }

    public abstract IEnumerator OnTick();

    public virtual void CleanUp() {
      WaitUtil.ReleaseWaitUntil(waitPlayCard);
      waitPlayCard = null;
      blackboard.Clear();
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

    public virtual void EnqueueCard(Card card) {
      if (card != null) { // 注意结束回合的标志是一个空的卡牌这个不能统计进去
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
    }
  }
}