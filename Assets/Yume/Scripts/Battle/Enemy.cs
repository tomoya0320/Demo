using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yume.Data;
using Yume.Resource;
using Object = UnityEngine.Object;

namespace Yume.Battle {
  public class Enemy : Unit {
    protected List<Card> cards = new List<Card>(); // 敌人的卡牌都放一起

    public Enemy(UnitData enemy, CardData[] cards) : base(enemy, BattleManager.enemyFolder) {
      var prefab = ResourceManager.Load<GameObject>("card_prefab");
      foreach (var card in cards) {
        var go = Object.Instantiate(prefab, gameObject.transform); // 敌人的牌就放自己的下面
        this.cards.Add(new Card(card, go));
      }
    }

    public override IEnumerator OnTick() {
      // TODO:测试用
      yield return new WaitForSeconds(2);
      // 筛选出符合出牌条件的牌
      List<Card> candidateCards = cards.FindAll(card => card.cardPhase.energyConsumption <= GetAttrib(AttribType.ENERGY));
      // 没有就直接结束回合
      if (candidateCards.Count == 0) {
        yield break;
      }
      // 随机选一张牌出
      int index = MathUtil.random.Next(0, candidateCards.Count);
      playCardQueue.Enqueue((candidateCards[index], BattleManager.character));
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
  }
}