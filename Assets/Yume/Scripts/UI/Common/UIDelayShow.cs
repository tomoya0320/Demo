using System.Collections;
using UnityEngine;

namespace Yume.UI {
  [RequireComponent(typeof(CanvasGroup))]
  public class UIDelayShow : MonoBehaviour {
    [SerializeField]
    private float _delayTime = 1.0f;
    private CanvasGroup m_canvasGroup;

    private void Awake() {
      m_canvasGroup = GetComponent<CanvasGroup>();
    }

    private void OnEnable() {
      m_canvasGroup.alpha = 0;
      StartCoroutine(_DelayShow());
    }

    private IEnumerator _DelayShow() {
      yield return new WaitForSeconds(_delayTime);
      m_canvasGroup.alpha = 1.0f;
    }
  }
}