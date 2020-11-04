using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Yume.AVG;
using Yume.DB;
using Yume.MailSystem;
using Yume.Scene;
using Yume.Singleton;
using Yume.String;

namespace Yume.UI {
  public class UIController : SingletonMono<UIController> {
    [Serializable]
    private class ConfirmationBox {
      public UIFade ui;
      public Text tip;
      public WButton ok;
      public WButton cancel;
    }

    [SerializeField]
    private ConfirmationBox _confirmationBox;  
    [SerializeField]
    private Image _blackMask;
    [SerializeField]
    private GameObject _sceneLoadingBG;
    [SerializeField]
    private UIFade _httpRequestWaitingMask;
    [SerializeField]
    private List<MessageBox> _messageBoxes;
    public List<MessageBox> messageBoxes => _messageBoxes;
    public MessageManager messageManager { private set; get; }

    public bool sceneLoadingBGActive {
      set {
        _sceneLoadingBG.SetActive(value);
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

    private IList<IUIDisplay> m_UIStack;
    private Tween m_blackMaskTween;

    public bool activeEscapeKey { set; get; }

    protected override void OnInit() {
      base.OnInit();
      activeEscapeKey = true;
      m_UIStack = new List<IUIDisplay>();
      messageManager = new MessageManager();
    }

    private void Update() {
      if (activeEscapeKey && Input.GetKeyDown(KeyCode.Escape)) {
        if (m_UIStack.Count == 0) {
          OpenConfirmationBox(StringMap.QUIT_GAME_TIP, StringMap.QUIT, StringMap.CANCEL, Application.Quit);
        } else {
          if (!m_UIStack[m_UIStack.Count - 1].isTweenPlaying) {
            m_UIStack[m_UIStack.Count - 1].Close();
          }
        }
      }
#if TEST
      if (Input.GetKeyDown(KeyCode.A)) {
        DBManager.AddItems(new ItemDB("item_2_gold", 1000));
      }

      if (Input.GetKeyDown(KeyCode.S)) {
        DBManager.ConsumeItems(true, new ItemDB("item_2_gold", 1000));
      }

      if (Input.GetKeyDown(KeyCode.D)) {
        DBManager.AddItems(new ItemDB("item_1_diamond", 1));
      }

      if (Input.GetKeyDown(KeyCode.C)) {
        DBManager.setUserToken = "1d";
      }

      if (Input.GetKeyDown(KeyCode.M)) {
        DBManager.AddMail(new MailDB("Test Title", "Welcome to FangYuanQiWu!", "admin", new ItemDB("item_1_diamond", 1000)));
        DBManager.AddMail(new MailDB("Test Title2", "You are alone", "admin", new ItemDB("item_2_gold", 10000)));
        DBManager.AddMail(new MailDB("Test Title3", "I love {0}", "wzb", new ItemDB("item_5_key", 10)));
      }

      if (Input.GetKeyDown(KeyCode.E)) {
        DBManager.AddExp(300);
      }

      if (Input.GetKeyDown(KeyCode.L)) {
        messageManager.AddMessage(new Message(Message.Style.SLIDE, "testtesttest"));
      }

      if (Input.GetKeyDown(KeyCode.G)) {
        SceneLoader.GoToAVGScene("test1");
      }
#endif
    }

    public void FadeBlackMask(bool active, float fadeTime = 0.3f, Action onComplete = null) {
      if (m_blackMaskTween != null) {
        m_blackMaskTween.Complete();
      }
      _blackMask.SetAlpha(active ? 0 : 1.0f);
      _blackMask.enabled = true;
      m_blackMaskTween = _blackMask.DOFade(active ? 1.0f : 0, fadeTime).OnComplete(() => {
        _blackMask.enabled = false;
        m_blackMaskTween = null;
        onComplete?.Invoke();
      });
    }

    public void AddToBackStack(IUIDisplay ui) {
      if (!m_UIStack.Contains(ui)) {
        m_UIStack.Add(ui);
      }
    }

    public void RemoveFromBackStack(IUIDisplay ui) {
      if (m_UIStack.Contains(ui)) {
        m_UIStack.Remove(ui);
      }
    }

    public void ClearBackStack() {
      m_UIStack.Clear();
    }

    public void OpenConfirmationBox(string tip, string okText, string cancelText, UnityAction onOk = null, UnityAction onCancel = null) {
      _confirmationBox.tip.text = tip;

      #region ok
      _confirmationBox.ok.textComponent.text = okText;
      _confirmationBox.ok.onClick.RemoveAllListeners();
      _confirmationBox.ok.onClick.AddListener(() => _confirmationBox.ui.Close());
      if (onOk != null) {
        _confirmationBox.ok.onClick.AddListener(onOk);
      }
      #endregion

      #region cancel
      _confirmationBox.cancel.textComponent.text = cancelText;
      _confirmationBox.cancel.onClick.RemoveAllListeners();
      _confirmationBox.cancel.onClick.AddListener(() => _confirmationBox.ui.Close());
      if (onCancel != null) {
        _confirmationBox.cancel.onClick.AddListener(onCancel);
      }
      #endregion

      _confirmationBox.ui.Open();
    }
  }
}