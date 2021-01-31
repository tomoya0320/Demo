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
    public CanvasGroup dialogueCanvasGroup { private set; get; }
    #endregion

    private Action m_onQuitAVG;
    private AVGGraph m_avgGraph;
    private string m_beforeBgmName;

    public override void OnInit(UITemplate uiTemplate, GameObject root) {
      base.OnInit(uiTemplate, root);

      bgImage = GetComponent<Image>("bg");
      nameText = GetComponent<Text>("name");
      dialogueText = GetComponent<Text>("dialogue");
      illustration = GetComponent<Image>("illustration");
      dialogueCanvasGroup = GetComponent<CanvasGroup>("dialogueCanvasGroup");

      _AddListener();
    }

    private void _AddListener() {
      GetComponent<Button>("skip").onClick.AddListener(_SkipAVG);
      GetComponent<Button>("DoOperation").onClick.AddListener(_DoOperation);
    }

    public override void Open(Action callback = null, params object[] args) {
      base.Open(callback, args);

      _InitAVG((string)args[0], args[1] as Action);
      _DoOperation();

      UIManager.onBackStack.Push(_SkipAVG);
    }

    public override void Close(Action callback = null, params object[] args) {
      callback += () => {
        _CleanUp();
        m_onQuitAVG?.Invoke();
        UIManager.onBackStack.Pop();
        AudioManager.PlayBgm(m_beforeBgmName);
      };

      base.Close(callback, args);
    }

    private void _SkipAVG() {
      if (UIManager.GetTopUI(uiType).name == name) {
        UIManager.OpenOne("confirmation", null, "确认跳过剧情？", "确认", "取消", new Action(QuitAVG), null);
      } else {
        UIManager.CloseOne("confirmation");
      }
    }

    private void _DoOperation() {
      m_avgGraph.Run(this);
    }

    public void QuitAVG() {
      UIManager.OpenOne("globalBlackFade", () => UIManager.CloseOne(name));
    }

    private void _InitAVG(string avgName, Action onQuitAVG) {
      m_onQuitAVG = onQuitAVG;
      m_beforeBgmName = AudioManager.GetCurrentBgmName();
      m_avgGraph = ResourceManager.Load<AVGGraph>(avgName).Init();
    }

    private void _CleanUp() {
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