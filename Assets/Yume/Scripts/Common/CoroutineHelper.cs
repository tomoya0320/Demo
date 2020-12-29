using System;
using System.Collections;
using UnityEngine;

namespace Yume.Tool {
  public class CoroutineHelper : SingletonMono<CoroutineHelper> {
    public void RunCoroutine(IEnumerator enumerator) {
      StartCoroutine(enumerator);
    }

    public void AnimCallback(Animation animation, Action callback) {
      RunCoroutine(_AnimCallbackInternal(animation, callback));
    }

    private IEnumerator _AnimCallbackInternal(Animation animation, Action callback) {
      yield return new WaitUntil(() => !animation.isPlaying);
      callback?.Invoke();
    }
  }
}