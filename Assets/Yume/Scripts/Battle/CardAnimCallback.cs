using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yume.Audio;

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
      yield return new WaitUntil(() => !m_animation.isPlaying);
      gameObject.SetActiveEx(false);
    }

    private void _OnCast() {
      if (card == null) {
        Debug.LogError("card is not initialized!");
        return;
      }
      
      foreach (var buffTemplate in card.cardPhase.buffTemplates) {
        // TODO:以caster和candidate为基准筛选出目标 测试用
        List<Unit> targets = buffTemplate.selector.Select(card.owner, card.candidate);
        foreach (var target in targets) {
          // 跳过已死亡的单位
          if (!target.isAlive) {
            continue;
          }
          
          // 因为blackboard是引用类型所以需要新建一个
          Blackboard blackboard = new Blackboard { { "caster", card.owner } };
          blackboard = blackboard.Merge(buffTemplate.blackboard);
          BattleManager.buffManagers[target].AddBuff(buffTemplate.id, buffTemplate.stackType, blackboard, buffTemplate.behaviors);
        }
      }
    }

    private void _OnPlaySound(string sound) {
      AudioManager.PlaySound(sound);
    }
  }
}