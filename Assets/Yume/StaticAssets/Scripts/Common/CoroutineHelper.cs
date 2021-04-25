using System;
using System.Collections;
using UnityEngine;

namespace Yume {
  public class CoroutineHelper : SingletonMono<CoroutineHelper> {
    public Coroutine Run(IEnumerator enumerator) {
      return StartCoroutine(enumerator);
    }

    public Coroutine DelayInvoke(float time, Action callback) {
      return StartCoroutine(_DelayInvokeInternal(time, callback));
    }

    public void Stop(Coroutine routine) {
      StopCoroutine(routine);
    }

    private IEnumerator _DelayInvokeInternal(float time, Action callback) {
      yield return new WaitForSeconds(time);
      callback?.Invoke();
    }

    /// <summary>
    /// 播放一个带回调的动画
    /// 动画播放完毕执行回调
    /// 中途被打断的话无法执行
    /// </summary>
    /// <param name="clip"></param>
    /// <param name="animation"></param>
    /// <param name="callback"></param>
    public void PlayAnimCallback(AnimationClip clip, Animation animation, Action callback) {
      animation.clip = clip;
      animation.Play();

      Run(_AnimCallback(clip.name, animation, callback));
    }

    private IEnumerator _AnimCallback(string clipName, Animation animation, Action callback) {
      while (animation.isPlaying) {
        //保证播放的是同一段动画
        if (animation.clip.name == clipName) {
          yield return null;
        } else {
          yield break;
        }
      }
      callback?.Invoke();
    }
  }
}