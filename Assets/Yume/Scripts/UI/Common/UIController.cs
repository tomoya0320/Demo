using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using Yume.Singleton;

namespace Yume.UI {
  public class UIController : SingletonMono<UIController> {
    [SerializeField]
    private UIFade _confirmation;//quit game confirmation
    [SerializeField]
    private CanvasGroup _blackMask;
    [SerializeField]
    private GameObject _sceneLoadingMask;   
    [SerializeField]
    private UIFade _httpRequestWaitingMask;
    [SerializeField]
    private Transform _messageFolder;
    public Transform messageFolder { get { return _messageFolder; } }
    public MessageManager messageManager { private set; get; }

    public bool sceneLoadingMaskActive {
      set {
        _sceneLoadingMask.SetActive(value);
      }
    }
    public bool httpRequestWaitingMaskActive {
      set {
        if (value) {
          _httpRequestWaitingMask.Open();
        } else {
          _httpRequestWaitingMask.Close();
        }
      }
    }

    private IList<IUIDisplay> m_UIStack = new List<IUIDisplay>();
    private Tween m_blackMaskTween;

    protected override void OnInit() {
      base.OnInit();
      messageManager = new MessageManager();
    }

    private void Update() {
      if (Input.GetKeyDown(KeyCode.Escape)) {
        if (m_UIStack.Count == 0) {
          _confirmation.Open();
        } else {
          m_UIStack[m_UIStack.Count - 1].Close();
        }
      }
    }

    public void QuitGame() {
      Application.Quit();
    }

    public void FadeBlackMask(bool active, float fadeTime = 0.3f, Action onComplete = null) {
      if (m_blackMaskTween != null) {
        m_blackMaskTween.Complete();
      }
      _blackMask.alpha = active ? 0 : 1.0f;
      _blackMask.gameObject.SetActive(true);
      m_blackMaskTween = _blackMask.DOFade(active ? 1.0f : 0, fadeTime).OnComplete(() => {
        _blackMask.gameObject.SetActive(false);
        m_blackMaskTween = null;
        onComplete?.Invoke();
      });
    }

    public void AddToBackStack(IUIDisplay ui) {
      m_UIStack.Add(ui);
    }

    public void RemoveFromBackStack(IUIDisplay ui) {
      m_UIStack.Remove(ui);
    }

    public void ClearBackStack() {
      m_UIStack.Clear();
    }
  }
}