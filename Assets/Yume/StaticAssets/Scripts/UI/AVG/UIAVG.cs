using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using Yume.AVG;

namespace Yume.UI {
  public class UIAVG : UIBase {
    #region Component

    public Image bgImage { private set; get; }
    public Text nameText { private set; get; }
    public Button skipButton { private set; get; }
    public Text dialogueText { private set; get; }
    public Image illustration { private set; get; }
    public YButton autoButton { private set; get; }
    public YButton[] optionBtns { private set; get; }
    public InputField inputField { private set; get; }
    public Button skipVideoButton { private set; get; }
    public Button inputFieldOkBtn { private set; get; }
    public RawImage videoRawImage { private set; get; }
    public VideoPlayer videoPlayer { private set; get; }
    public CanvasGroup videoCanvasGroup { private set; get; }
    public CanvasGroup optionCanvasGroup { private set; get; }
    public CanvasGroup dialogueCanvasGroup { private set; get; }
    public CanvasGroup inputFieldCanvasGroup { private set; get; }

    #endregion

    private bool m_auto;
    public Action onQuitAVG;
    public string playerName; // 与真正的PlayerName独立开来方便一些特殊的需求
    private AVGGraph m_avgGraph;
    public string historyDialogues;
    private Coroutine m_autoCoroutine;
    private const float AUTO_WAITING_TIME = 3.0f;

    public override void OnInit(UITemplate uiTemplate, GameObject gameObject) {
      base.OnInit(uiTemplate, gameObject);

      skipButton = GetComponent<Button>("skip");
      skipVideoButton = GetComponent<Button>("skipVideo");

      bgImage = GetComponent<Image>("bg");
      nameText = GetComponent<Text>("name");
      autoButton = GetComponent<YButton>("auto");
      dialogueText = GetComponent<Text>("dialogue");
      illustration = GetComponent<Image>("illustration");
      inputField = GetComponent<InputField>("inputField");
      videoPlayer = GetComponent<VideoPlayer>("videoPlayer");
      videoRawImage = GetComponent<RawImage>("videoRawImage");
      optionCanvasGroup = GetComponent<CanvasGroup>("options");
      inputFieldOkBtn = GetComponent<Button>("inputFieldOkBtn");
      videoCanvasGroup = GetComponent<CanvasGroup>("videoCanvasGroup");
      dialogueCanvasGroup = GetComponent<CanvasGroup>("dialogueCanvasGroup");
      inputFieldCanvasGroup = GetComponent<CanvasGroup>("inputFieldCanvasGroup");

      optionBtns = optionCanvasGroup.GetComponentsInChildren<YButton>(true);

      videoPlayer.targetCamera = Camera.main;

      _AddListener();
    }

    private void _AddListener() {
      GetComponent<Button>("DoOperation").onClick.AddListener(_DoOperation);
      GetComponent<Button>("history").onClick.AddListener(
        () =>
          UIManager.OpenOne("avg_history", null, historyDialogues)
      );

      autoButton.onClick.AddListener(() => _SetAutoState(!m_auto));
      skipButton.onClick.AddListener(Skip);
      skipVideoButton.onClick.AddListener(SkipVideo);
    }

    private void _CheckSkipButtons() {
      skipButton.gameObject.SetActiveEx(!m_avgGraph.disableSkip);
      skipVideoButton.gameObject.SetActiveEx(!m_avgGraph.disableSkipVideo);
    }

    private void _SetAutoState(bool state) {
      m_auto = state;
      autoButton.textComponent.text = m_auto ? "自动:开" : "自动:关";
    }

    private IEnumerator _Auto() {
      while (true) {
        if (m_avgGraph.block <= 0) {
          float startTime = Time.time;
          while (Time.time < startTime + AUTO_WAITING_TIME) {
            if (!m_auto) {
              break;
            }
            yield return null;
          }
          if (m_auto) {
            // 当玩家在进行其他操作的时候等待
            yield return new WaitUntil(() => UIManager.GetCurrentUI().name == name);
            m_avgGraph.Run();
          }
        }
        yield return null;
      }
    }

    public override void Open(Action callback = null, params object[] args) {
      base.Open(callback, args);

      m_avgGraph = ResourceManager.Load<AVGGraph>(args[0] as string);
      onQuitAVG = args[1] as Action;
      playerName = DataManager.playerName;

      // 检查是否激活跳过按钮
      _CheckSkipButtons();

      UIManager.onBackList.Add(Skip);

      m_avgGraph.avg = this;
      m_avgGraph.Enter();
      m_autoCoroutine = CoroutineHelper.instance.Run(_Auto());
    }

    public void Skip() {
      if (m_avgGraph.disableSkip) {
        return;
      }

      if (UIManager.GetCurrentUI().name == name) {
        Action action = QuitAVG;
        UIManager.OpenOne("confirmation", null, "确认跳过剧情？", "确认", "取消", action, null);
      } else {
        UIManager.CloseOne(UIManager.GetCurrentUI().name);
      }
    }

    public void SkipVideo() {
      if (m_avgGraph.disableSkipVideo) {
        return;
      }

      if (UIManager.GetCurrentUI().name == name) {
        Action action = videoPlayer.Stop;
        UIManager.OpenOne("confirmation", null, "确认跳过视频？", "确认", "取消", action, null);
      } else {
        UIManager.CloseOne(UIManager.GetCurrentUI().name);
      }
    }

    private void _DoOperation() {
      if (m_auto) {
        _SetAutoState(false);
      }
      m_avgGraph.Run();
    }

    public void QuitAVG() {
      UIManager.GlobalFade(GlobalFadeType.BLACK_FADE, _OnQuitAVG + onQuitAVG);
    }

    private void _OnQuitAVG() {
      foreach (var dialogOptionBtn in optionBtns) {
        dialogOptionBtn.textComponent.text = StringMap.EMPTY;
        dialogOptionBtn.onClick.RemoveAllListeners();
      }
      playerName = StringMap.EMPTY;
      inputField.text = StringMap.EMPTY;
      inputFieldOkBtn.onClick.RemoveAllListeners();
      inputFieldCanvasGroup.alpha = 0;
      inputFieldCanvasGroup.blocksRaycasts = false; 
      optionCanvasGroup.alpha = 0;
      optionCanvasGroup.blocksRaycasts = false;
      bgImage.color = Color.white;
      nameText.text = StringMap.EMPTY;
      illustration.color = Color.white;
      dialogueCanvasGroup.alpha = 1.0f;
      bgImage.SetSprite(StringMap.NO_SPRITE);
      illustration.SetSprite(StringMap.NO_SPRITE);
      
      historyDialogues = StringMap.EMPTY;
      dialogueText.text = StringMap.EMPTY;
      CoroutineHelper.instance.Stop(m_autoCoroutine);
      m_autoCoroutine = null;
      _SetAutoState(false);
      
      m_avgGraph.CleanUp();
      
      UIManager.onBackList.Remove(Skip);
      UIManager.CloseOne(name, true);
    }
  }
}