using System;
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
    public GameObject dialogOptions { private set; get; }
    public YButton[] dialogOptionBtns { private set; get; }
    public CanvasGroup dialogueCanvasGroup { private set; get; }
    #endregion

    private Action m_onQuitAVG;
    private AVGGraph m_avgGraph;

    public override void OnInit(UITemplate uiTemplate, GameObject gameObject) {
      base.OnInit(uiTemplate, gameObject);

      bgImage = GetComponent<Image>("bg");
      nameText = GetComponent<Text>("name");
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
    }

    public override void Open(Action callback = null, params object[] args) {
      base.Open(callback, args);
      
      m_avgGraph = ResourceManager.Load<AVGGraph>((string)args[0]);
      m_onQuitAVG = args[1] as Action;
      m_avgGraph.Enter(this);
      
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
      dialogueText.text = StringMap.EMPTY;
      bgImage.sprite = ResourceManager.Load<Sprite>("noSprite");
      illustration.sprite = ResourceManager.Load<Sprite>("noSprite");
    }
  }
}