using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Yume.Audio;
using Yume.AVG;
using Yume.Data;
using Yume.Resource;

namespace Yume.UI {
  public class UIAVG : UIBase {
    #region Component
    
    public Image bgImage { private set; get; }
    public Text nameText { private set; get; }
    public Text dialogueText { private set; get; }
    public Image illustration { private set; get; }
    public YButton autoButton { private set; get; }
    public GameObject dialogOptions { private set; get; }
    public YButton[] dialogOptionBtns { private set; get; }
    public CanvasGroup dialogueCanvasGroup { private set; get; }
    
    #endregion

    private bool m_auto;
    private Action m_onQuitAVG;
    private AVGGraph m_avgGraph;
    public string historyDialogues;
    private Coroutine m_autoCoroutine;
    private const float AUTO_WAITING_TIME = 3.0f;

    public override void OnInit(UITemplate uiTemplate, GameObject gameObject) {
      base.OnInit(uiTemplate, gameObject);

      bgImage = GetComponent<Image>("bg");
      nameText = GetComponent<Text>("name");
      autoButton = GetComponent<YButton>("auto");
      dialogueText = GetComponent<Text>("dialogue");
      dialogOptions = GetGameObject("DialogOptions");
      illustration = GetComponent<Image>("illustration");
      dialogueCanvasGroup = GetComponent<CanvasGroup>("dialogueCanvasGroup");

      dialogOptionBtns = dialogOptions.GetComponentsInChildren<YButton>(true);

      _AddListener();
    }

    private void _AddListener() {
      GetComponent<Button>("skip").onClick.AddListener(_SkipAVG);
      GetComponent<Button>("DoOperation").onClick.AddListener(_DoOperation);
      GetComponent<Button>("history").onClick.AddListener(
        () =>
          UIManager.OpenOne("avg_history", null, historyDialogues)
      );
      autoButton.onClick.AddListener(() => _SetAutoState(!m_auto));
    }

    private void _SetAutoState(bool state) {
      m_auto = state;
      if (m_auto) {
        autoButton.textComponent.text = "自动:开";
      } else {
        autoButton.textComponent.text = "自动:关";
      }
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
            m_avgGraph.Run(this);
          }
        }
        yield return null;
      }
    }

    public override void Open(Action callback = null, params object[] args) {
      base.Open(callback, args);
      
      m_avgGraph = ResourceManager.Load<AVGGraph>((string)args[0]);
      m_onQuitAVG = args[1] as Action;
      m_avgGraph.Enter(this);
      m_autoCoroutine = CoroutineHelper.instance.Run(_Auto());
      
      UIManager.onBackList.Add(_SkipAVG);
    }

    public override void Close(Action callback = null, params object[] args) {
      callback += () => {
        _CleanUp();
        m_onQuitAVG?.Invoke();
        UIManager.onBackList.Remove(_SkipAVG);
        // TODO:临时用
        AudioManager.PlayBgm(StringMap.DEFAULT_BGM);
      };

      base.Close(callback, args);
    }

    private void _SkipAVG() {
      if (UIManager.GetTopUI(uiType).name == name) {
        UIManager.OpenOne("confirmation", null, "确认跳过剧情？", "确认", "取消", new Action(QuitAVG), null);
      } else {
        UIManager.CloseOne(UIManager.GetTopUI(uiType).name);
      }
    }

    private void _DoOperation() {
      if (m_auto) {
        _SetAutoState(false);
      }
      m_avgGraph.Run(this);
    }

    public void QuitAVG() {
      UIManager.OpenOne("globalBlackFade", () => UIManager.CloseOne(name));
    }

    private void _CleanUp() {
      m_avgGraph.CleanUp();
      foreach (var dialogOptionBtn in dialogOptionBtns) {
        dialogOptionBtn.textComponent.text = StringMap.EMPTY;
        dialogOptionBtn.onClick.RemoveAllListeners();
      }
      dialogOptions.SetActiveEx(false);
      bgImage.color = Color.white;
      nameText.text = StringMap.EMPTY;
      illustration.color = Color.white;
      dialogueCanvasGroup.alpha = 1.0f;
      historyDialogues = StringMap.EMPTY;
      dialogueText.text = StringMap.EMPTY;
      bgImage.sprite = ResourceManager.Load<Sprite>("noSprite");
      illustration.sprite = ResourceManager.Load<Sprite>("noSprite");
      CoroutineHelper.instance.Stop(m_autoCoroutine);
      m_autoCoroutine = null;
    }
  }
}