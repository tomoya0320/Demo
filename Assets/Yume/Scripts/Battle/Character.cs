using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.UI;
using Yume.Data;
using Yume.Resource;
using Yume.UI;
using Object = UnityEngine.Object;

namespace Yume.Battle {
  public enum CardHeapType {
    /// <summary>
    /// 抽牌堆
    /// </summary>
    DRAW,
    /// <summary>
    /// 手牌堆
    /// </summary>
    HAND,
    /// <summary>
    /// 弃牌堆
    /// </summary>
    DISCARD,
    /// <summary>
    /// 耗牌堆
    /// </summary>
    CONSUME,
  }
  
  public class Character : Unit {
    private Text m_hpText;
    private Text m_energyText;
    private List<Card> m_tempCardList = new List<Card>();
    protected Dictionary<CardHeapType, List<Card>> cardHeapDict = new Dictionary<CardHeapType, List<Card>>(); //牌堆(抽牌、弃牌、耗牌、手牌)

    public Character(UnitData character, CardData[] cards) : base(character, BattleManager.charFolder) {
      foreach (CardHeapType cardHeapType in Enum.GetValues(typeof(CardHeapType))) {
        cardHeapDict.Add(cardHeapType, new List<Card>());
      }

      var prefab = ResourceManager.Load<GameObject>(StringMap.CARD_PREFAB);
      foreach (var cardData in cards) {
        var go = Object.Instantiate(prefab, BattleManager.cardFolder);
        cardHeapDict[CardHeapType.DRAW].Add(new Card(cardData, go, /*测试用*/card => {
          if (!card.active || !BattleManager.isRunning) {
            return;
          }
          if (card.cardPhase.energyConsumption > GetAttribValue(AttribType.ENERGY)) {
            MessageManager.AddMessage(new Message(Message.Style.CENTER, "能量不足！", null, 0.5f, false));
            return;
          }
          
          EnqueueCard((card, BattleManager.enemy));
          card.active = false;
        }));
      }

      // 初始化UI
      var objectContainer = gameObject.GetComponent<ObjectContainer>();
      m_hpText = objectContainer.GetComponent<Text>("hp");
      m_energyText = objectContainer.GetComponent<Text>("energy");

      m_hpText.text = $"生命:{GetAttribValue(AttribType.HP)}/{GetAttribMaxValue(AttribType.HP)}";
      m_energyText.text = $"能量:{GetAttribValue(AttribType.ENERGY)}/{GetAttribMaxValue(AttribType.ENERGY)}";
    }

    public override IEnumerator OnTrick() {
      // 补充能量
      float energy = GetAttribValue(AttribType.ENERGY);
      float maxEnergy = GetAttribMaxValue(AttribType.ENERGY);
      if (energy < maxEnergy) {
        AddAttribValue(AttribType.ENERGY, maxEnergy - energy);
      }

      // 抽牌逻辑层
      int deltaCardCount = Constant.HAND_CARD_THRESHOLD - cardHeapDict[CardHeapType.HAND].Count;
      if (deltaCardCount > 0) {
        if (deltaCardCount > cardHeapDict[CardHeapType.DRAW].Count && 
            cardHeapDict[CardHeapType.DISCARD].Count > 0) {
          // 弃牌堆的牌重新进入抽牌堆
          cardHeapDict[CardHeapType.DRAW].AddRange(cardHeapDict[CardHeapType.DISCARD]);
          cardHeapDict[CardHeapType.DISCARD].Clear();
        }
        
        Card[] randomCards = MathUtil.FisherYatesShuffle(cardHeapDict[CardHeapType.DRAW].ToArray());
        foreach (Card card in randomCards) {
          // 记录抽取的卡牌
          m_tempCardList.Add(card);
          cardHeapDict[CardHeapType.HAND].Add(card);
          cardHeapDict[CardHeapType.DRAW].Remove(card);
          if (cardHeapDict[CardHeapType.HAND].Count >= Constant.HAND_CARD_THRESHOLD) {
            break;
          }
        }
      }
      // 抽牌表现层
      // TODO:抽牌动效,现在这个暂时测试用
      m_tempCardList.ForEach(card=>card.gameObject.SetActiveEx(true));
      m_tempCardList.Clear();
      
      // 出牌中
      while (true) {
        // TODO:测试用
        if (BattleManager.battleResult != BattleResult.NONE) {
          yield break;
        }
        if (playCardQueue.Count > 0) {
          var cardTuple = playCardQueue.Dequeue();
          if (cardTuple == default) {
            break;
          } 
          cardTuple.card.Cast(this, cardTuple.target);
          // 把用过的牌弃掉
          // TODO:测试用,还没动效
          cardTuple.card.gameObject.SetActiveEx(false);
          cardTuple.card.active = true;
        }
        yield return null;
      }
      
      // 弃牌逻辑层
      // 记录弃置的卡牌
      m_tempCardList.AddRange(cardHeapDict[CardHeapType.HAND]);
      cardHeapDict[CardHeapType.DISCARD].AddRange(m_tempCardList);
      cardHeapDict[CardHeapType.HAND].Clear();
      // 弃牌表现层
      // TODO:弃牌动效,现在这个暂时测试用
      m_tempCardList.ForEach(card=>card.gameObject.SetActiveEx(false));
      m_tempCardList.Clear();
    }
    
    public override void OnDie() {
      // TODO:测试用
      BattleManager.battleResult = BattleResult.LOSE;
    }

    public override void CleanUp() {
      cardHeapDict.Values.ForEach(
        cards =>
          cards.ForEach(card => Object.Destroy(card.gameObject))
      );
      Object.Destroy(gameObject);
    }

    protected override void OnAttribValueChanged(AttribType type, float delta) {
      switch (type) {
        case AttribType.HP: {
          // 更新UI
          m_hpText.text = $"HP:{GetAttribValue(AttribType.HP):0}/{GetAttribMaxValue(AttribType.HP)}";
          break;
        }
        case AttribType.ATK:
          break;
        case AttribType.ENERGY:
          // 更新UI
          m_energyText.text = $"能量:{GetAttribValue(AttribType.ENERGY)}/{GetAttribMaxValue(AttribType.ENERGY)}";
          break;
        case AttribType.DEF:
          break;
      }
    }

    protected override void OnAttribMaxValueChanged(AttribType type, float delta) {
      switch (type) {
        case AttribType.HP: {
          // 更新UI
          m_hpText.text = $"HP:{GetAttribValue(AttribType.HP):0}/{GetAttribMaxValue(AttribType.HP)}";
          break;
        }
        case AttribType.ATK:
          break;
        case AttribType.ENERGY:
          // 更新UI
          m_energyText.text = $"能量:{GetAttribValue(AttribType.ENERGY)}/{GetAttribMaxValue(AttribType.ENERGY)}";
          break;
        case AttribType.DEF:
          break;
      }
    }
  }
}