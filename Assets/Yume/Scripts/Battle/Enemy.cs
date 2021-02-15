using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Yume.Data;
using Yume.Resource;
using Object = UnityEngine.Object;

namespace Yume.Battle {
  public class Enemy : Unit {
    private Text m_hpText;
    protected List<Card> cards = new List<Card>(); // 敌人的卡牌都放一起

    public Enemy(UnitData enemy, CardData[] cards) : base(enemy, BattleManager.enemyFolder) {
      var prefab = ResourceManager.Load<GameObject>(StringMap.CARD_PREFAB);
      foreach (var card in cards) {
        var go = Object.Instantiate(prefab, gameObject.transform); // 敌人的牌就放自己的下面
        this.cards.Add(new Card(card, go));
      }

      // 初始化UI
      var objectContainer = gameObject.GetComponent<ObjectContainer>();
      m_hpText = objectContainer.GetComponent<Text>("hp");

      m_hpText.text = $"HP:{GetAttribValue(AttribType.HP):0}/{GetAttribMaxValue(AttribType.HP):0}";
    }

    public override IEnumerator OnTrick() {
#if BETA
      if (cards.Count == 0) {
        yield break;
      }
#endif
      // 随机选一张牌出
      int index = MathUtil.random.Next(0, cards.Count);
      playCardQueue.Enqueue((cards[index], BattleManager.character));
      // 结束出牌的标记
      playCardQueue.Enqueue(default);
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
          // TODO:测试用
          cardTuple.card.gameObject.SetActiveEx(true);
          cardTuple.card.Cast(this, cardTuple.target);
          // TODO:测试用
          yield return new WaitForSeconds(2);
          cardTuple.card.gameObject.SetActiveEx(false);
          cardTuple.card.active = true;
        }
        yield return null;
      }
    }

    public override void OnDie() {
      // TODO:测试用
      BattleManager.battleResult = BattleResult.WIN;
    }

    public override void CleanUp() {
      cards.ForEach(card => Object.Destroy(card.gameObject));
      Object.Destroy(gameObject);
    }

    protected override void OnAttribValueChanged(AttribType type, float delta) {
      switch (type) {
        case AttribType.HP: {
          // 更新UI
          m_hpText.text = $"HP:{GetAttribValue(AttribType.HP):0}/{GetAttribMaxValue(AttribType.HP):0}";
          break;
        }
        case AttribType.ATK:
          break;
        case AttribType.ENERGY:
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
          break;
        case AttribType.DEF:
          break;
      }
    }
  }
}