using System.Collections;
using UnityEngine;

namespace Yume.Battle {
  [RequireComponent(typeof(Animation))]
  public class CardAnimCallback : MonoBehaviour {
    public Card card { set; get; }
    private Animation m_animation;

    private void Awake() {
      m_animation = GetComponent<Animation>();
      m_animation.playAutomatically = false;
    }

    public IEnumerator PlayAnim() {
      CardAnimEvent cardAnimEvent = card.cardPhase.cardAnimEvent;
      int eventCount = cardAnimEvent.eventCount;
      if (cardAnimEvent.loadFromBlackboard) {
        eventCount = card.owner.blackboard.GetValue<int>($"{card.cardTemplate.id}:eventCount");
      }
      while (eventCount > 0) {
        gameObject.SetActiveEx(true);
        transform.position = card.candidate.gameObject.transform.position;
        m_animation.Play();
        if (cardAnimEvent.waitingTime > 0) {
          yield return new WaitForSeconds(cardAnimEvent.waitingTime);
        }
        bool result = _OnCast() && card.owner.isAlive;
        yield return new WaitUntil(() => !m_animation.isPlaying);
        gameObject.SetActiveEx(false);
        // 释放过程中如果自己死了或目标全都死了就直接结束
        if (result) {
          eventCount--;
        } else {
          yield break;
        }
      }
    }

    /// <summary>
    /// 使用卡牌
    /// </summary>
    /// <returns>如果目标在使用卡牌后全部死亡则返回false</returns>
    private bool _OnCast() {
      if (card == null) {
        Debug.LogError("card is not initialized!");
        return false;
      }
      
      foreach (var buffTemplate in card.cardPhase.buffTemplates) {
        int leftCount = 0;
        // 以caster和candidate为基准筛选出目标
        foreach (var target in buffTemplate.selector.Select(card.owner, card.candidate)) {
          // 跳过已死亡的单位
          if (!target.isAlive) {
            continue;
          }
          // 因为blackboard是引用类型所以需要新建一个
          Blackboard blackboard = new Blackboard { { "caster", card.owner } };
          blackboard.Merge(buffTemplate.blackboard);
          BattleManager.buffManagers[target].AddBuff(buffTemplate.id, buffTemplate.stackType, blackboard, buffTemplate.behaviors);
          if (target.isAlive) {
            leftCount++;
          }
        }
        if (leftCount == 0) {
          return false;
        }
      }
      return true;
    }

    private void _OnPlaySound(string sound) {
      AudioManager.PlaySound(sound);
    }
  }
}