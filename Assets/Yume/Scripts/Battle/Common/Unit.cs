using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yume.Data;
using Yume.Resource;
using Object = UnityEngine.Object;

namespace Yume.Battle {
  public abstract class Unit {
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

    protected int level;
    protected string id;
    protected string tag;
    protected string name;
    protected Attrib[] attribs;
    protected Animator animator;
    protected GameObject gameObject;
    protected Queue<Card> cardQueue = new Queue<Card>(); // 出牌队列
    protected Dictionary<CardHeapType, List<Card>> cardHeapDict = new Dictionary<CardHeapType, List<Card>>(); //牌堆(抽牌、弃牌、耗牌)

    public bool isAlive => attribs[(int)AttribType.HP].value > 0;

    public Unit(UnitData unit, List<CardData> cardList, Transform unitFolder) {
      var unitTemplate = ResourceManager.Load<UnitTemplate>(unit.id);
      id = unit.id;
      level = unit.level;
      tag = unitTemplate.tag;
      name = unitTemplate.name;
      attribs = MathUtil.CalculateAttribs(unit.id, unit.level);
      var prefab = ResourceManager.Load<GameObject>(unitTemplate.prefabName);
      gameObject = Object.Instantiate(prefab, unitFolder);
      animator = gameObject.GetComponent<Animator>();

      foreach (CardHeapType cardHeapType in Enum.GetValues(typeof(CardHeapType))) {
        cardHeapDict.Add(cardHeapType, new List<Card>());
      }

      //TODO:Init PassiveSkill
    }

    public abstract IEnumerator OnTrick();

    public void EnqueueCard(Card card) => cardQueue.Enqueue(card);
  }
}