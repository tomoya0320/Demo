using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Yume.UI;
using Object = UnityEngine.Object;

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

    #region 表现层

    protected YImage hpImage;
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
        () => UIManager.OpenOne("battle_unit_detail", null, this)
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
    
    public abstract void CleanUp();

    public virtual void OnDie() {
      animator.SetBool(AnimatorParams.DIE, true);
      if (this is Character) {
        // 战斗失败
        BattleManager.Finish(BattleResult.LOSE);
      } else if(this is Enemy){
        // 移除这个单位施加的buff
        foreach (var buffManager in BattleManager.buffManagers.Values) {
          // 只针对还存活的单位
          if (buffManager.unit.isAlive) {
            buffManager.OnCasterDie(this);
          }
        }
        // 判断是否胜利
        if (BattleManager.units.Count == 1) {
          BattleManager.Finish(BattleResult.WIN);
        }
      }
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
          // 更新UI
          hpImage.fillAmount = (float)GetAttrib(AttribType.HP) / GetAttrib(AttribType.HP, true);
          hpImage.textComponent.text = $"{GetAttrib(AttribType.HP)}/{GetAttrib(AttribType.HP, true)}";
          break;
        }
        case AttribType.ENERGY:
          // 更新UI
          energyImage.textComponent.text = $"{GetAttrib(AttribType.ENERGY)}/{GetAttrib(AttribType.ENERGY, true)}";
          break;
      }
    }

    public abstract void EnqueueCard(Card card);
  }
}