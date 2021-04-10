using System.Collections;
using UnityEngine;

namespace Yume.Battle {
  [RequireComponent(typeof(Animation))]
  public class CardAnimCallback : MonoBehaviour {
    private Card m_card;
    private Animation m_animation;

    private void Awake() {
      m_animation = GetComponent<Animation>();
      m_animation.playAutomatically = false;
    }

    public void SetCard(Card card) {
      m_card = card;
    }

    public IEnumerator PlayAnim() {
      CardAnimEvent cardAnimEvent = m_card.cardPhase.cardAnimEvent;
      int eventCount = m_card.owner.blackboard.GetValueAndRemove<int>($"{m_card.cardTemplate.id}:{Blackboard.EXTRA_EVENT_COUNT}");
      if (cardAnimEvent.loadFromBlackboard) {
        eventCount += m_card.owner.blackboard.GetValueAndRemove<int>($"{m_card.cardTemplate.id}:{Blackboard.EVENT_COUNT}");
      } else {
        eventCount += cardAnimEvent.eventCount;
      }
      while (eventCount > 0) {
        gameObject.SetActiveEx(true);
        transform.position = m_card.candidate.gameObject.transform.position;
        m_animation.Play();
        if (cardAnimEvent.waitingTime > 0) {
          yield return new WaitForSeconds(cardAnimEvent.waitingTime);
        }
        bool result = _OnCast() && m_card.owner.isAlive;
        yield return new WaitWhile(() => m_animation.isPlaying);
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
      if (m_card == null) {
        Debug.LogError("card is null!");
        return false;
      }
      
      foreach (var buffTemplate in m_card.cardPhase.buffTemplates) {
        int leftCount = 0;
        // 以caster和candidate为基准筛选出目标
        foreach (var target in buffTemplate.selector.Select(m_card.owner, m_card.candidate)) {
          // 跳过已死亡的单位
          if (!target.isAlive) {
            continue;
          }
          // 因为blackboard是引用类型所以需要新建一个
          Blackboard blackboard = new Blackboard { { Blackboard.CASTER, m_card.owner } };
          blackboard.Merge(buffTemplate.blackboard);
          BattleManager.buffManagers[target].AddBuff(
            buffTemplate.id, buffTemplate.stackType, blackboard, buffTemplate.behaviors, buffTemplate.display
          );
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