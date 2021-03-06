using System.Collections;
using UnityEngine;
using Yume.Audio;
using Yume.Data;

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
      gameObject.SetActiveEx(true);
      transform.position = card.candidate.gameObject.transform.position;
      m_animation.Play();
      for (int i = 0; i < card.cardPhase.cardAnimEvents.Length; i++) {
        var cardAnimEvent = card.cardPhase.cardAnimEvents[i];
        int eventCount = cardAnimEvent.eventCount;
        if (cardAnimEvent.loadFromBlackboard) {
          eventCount = card.owner.blackboard.GetValue<int>($"{card.cardTemplate.id}:eventCount-{i}");
        }
        while (eventCount > 0) {
          if (cardAnimEvent.waitingTime > 0) {
            yield return new WaitForSeconds(cardAnimEvent.waitingTime);
          }
          _OnCast(cardAnimEvent.buffIndex);
          eventCount--;
          // 释放过程中如果自己死了就直接跳过
          if (!card.owner.isAlive) {
            // 注意goto只用于跳出多重循环其他地方不建议使用
            goto final;
          }
        }
      }
      final:
      yield return new WaitUntil(() => !m_animation.isPlaying);
      gameObject.SetActiveEx(false);
    }

    private void _OnCast(int buffIndex) {
      if (card == null) {
        Debug.LogError("card is not initialized!");
        return;
      }

      if (buffIndex >= card.cardPhase.buffTemplates.Length) {
        Debug.LogError("buffIndex is out of buffTemplates range!");
        return;
      }

      if (buffIndex < 0) {
        // buffIndex小于0表示所有的buff都加
        foreach (var buffTemplate in card.cardPhase.buffTemplates) {
          _AddBuff(buffTemplate);
        }
      } else {
        // 只加buffIndex指定的buff
        _AddBuff(card.cardPhase.buffTemplates[buffIndex]);
      }
    }

    private void _AddBuff(BuffTemplate buffTemplate) {
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
      }
    }

    private void _OnPlaySound(string sound) {
      AudioManager.PlaySound(sound);
    }
  }
}