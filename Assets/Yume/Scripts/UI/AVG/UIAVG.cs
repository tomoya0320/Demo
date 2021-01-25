using DG.Tweening;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Yume.Audio;
using Yume.Main;
using Yume.Resource;

namespace Yume.UI {
  public class UIAVG : UIBase {
    #region AVGOperation
    #region 无过渡的
    private abstract class AVGOperation {
      protected UIAVG avg;
      protected Dictionary<string, string> dataDict = new Dictionary<string, string>();

      public AVGOperation Init(UIAVG avg, JObject rawData) {
        this.avg = avg;
        foreach (var kv in rawData) {
          dataDict.Add(kv.Key, kv.Value.ToString());
        }
        return this;
      }

      public abstract void DoOperation();
    }

    private class SetDialogue : AVGOperation {
      public override void DoOperation() {
        avg.m_nameText.text = dataDict["name"];
        avg.m_dialogueText.text = StringMap.EMPTY;
        Tween tween = avg.m_dialogueText.DOText(dataDict["dialogue"], 20.0f).SetSpeedBased();
        tween.OnComplete(() => avg.m_tweenList.Remove(tween));
        avg.m_tweenList.Add(tween);
      }
    }

    private class SetBackground : AVGOperation {
      public override void DoOperation() {
        avg.m_bgImage.sprite = ResourceManager.Load<Sprite>(dataDict["bg"]);
      }
    }

    private class SetIllustration : AVGOperation {
      public override void DoOperation() {
        avg.m_illustration.sprite = ResourceManager.Load<Sprite>(dataDict["illustration"]);
      }
    }

    private class PlayBgm : AVGOperation {
      public override void DoOperation() {
        AudioManager.instance.PlayBgm(dataDict["bgm"]);
      }
    }

    private class PlaySound : AVGOperation {
      public override void DoOperation() {
        AudioManager.instance.PlaySound(dataDict["sound"]);
      }
    }
    #endregion

    #region 有过渡的
    private abstract class AVGFadeOperation : AVGOperation {
      public override void DoOperation() {
        avg.m_block++;
        Tween tween = DOTween(float.Parse(dataDict["fadeTime"]));
        tween.OnComplete(() => {
          avg.m_tweenList.Remove(tween);
          OnFadeCompleted();
        });
        avg.m_tweenList.Add(tween);
      }

      protected abstract Tween DOTween(float fadeTime);

      protected void OnFadeCompleted() {
        foreach (var item in JArray.Parse(dataDict["onFadeCompleted"])) {
          AVGOperation operation = avg._GetAVGOperation(item);
          operation.DoOperation();
        }
        avg.m_block--;
      }
    }

    private class SetDialogueVisible : AVGFadeOperation {
      protected override Tween DOTween(float fadeTime) {
        float alpha = bool.Parse(dataDict["visible"]) ? 1.0f : 0;
        return avg.m_dialogueCanvasGroup.DOFade(alpha, fadeTime);
      }
    }

    private class SetIllustrationVisible : AVGFadeOperation {
      protected override Tween DOTween(float fadeTime) {
        float alpha = bool.Parse(dataDict["visible"]) ? 1.0f : 0;
        return avg.m_illustration.DOFade(alpha, fadeTime);
      }
    }

    private class SetBackgroundColor : AVGFadeOperation {
      protected override Tween DOTween(float fadeTime) {
        Color color = JsonConvert.DeserializeObject<Color>(dataDict["color"]);
        return avg.m_bgImage.DOColor(color, fadeTime);
      }
    }
    #endregion
    #endregion

    #region Component
    private Image m_bgImage;
    private Text m_nameText;
    private Text m_dialogueText;
    private Image m_illustration;
    private CanvasGroup m_dialogueCanvasGroup;
    #endregion

    private int m_block;
    private Action m_onQuitAVG;
    private string m_beforeBgmName;
    private Queue<Queue<AVGOperation>> m_avgOperations;
    private List<Tween> m_tweenList = new List<Tween>();

    public override void OnInit(UIData uiData, GameObject root) {
      base.OnInit(uiData, root);

      m_bgImage = GetComponent<Image>("bg");
      m_nameText = GetComponent<Text>("name");
      m_dialogueText = GetComponent<Text>("dialogue");
      m_illustration = GetComponent<Image>("illustration");
      m_dialogueCanvasGroup = GetComponent<CanvasGroup>("dialogueCanvasGroup");

      _AddListener();
    }

    private void _AddListener() {
      GetComponent<Button>("skip").onClick.AddListener(SkipAVG);
      GetComponent<Button>("DoOperation").onClick.AddListener(_DoOperation);
    }

    public override void Open(Action callback = null, params object[] args) {
      base.Open(callback, args);

      _InitAVG((string)args[0], args[1] as Action);
      _DoOperation();
    }

    public void SkipAVG() {
      if (UIManager.IsShowing("confirmation")) {
        UIManager.CloseOne("confirmation");
      } else {
        UIManager.OpenOne("confirmation", null, "确认跳过剧情？", "确认", "取消", new Action(_QuitAVG), null);
      }
    }

    private AVGOperation _GetAVGOperation(JToken data) {
      string type = data["type"].Value<string>();
      JObject rawData = data["rawData"].Value<JObject>();
      AVGOperation operation = AssemblyTool.CreateInstance(type) as AVGOperation;
      return operation.Init(this, rawData);
    }

    private Queue<Queue<AVGOperation>> _InitAVGOperations(JArray arrayArray) {
      Queue<Queue<AVGOperation>> avgOperations = new Queue<Queue<AVGOperation>>();
      foreach (var array in arrayArray) {
        Queue<AVGOperation> tempQueue = new Queue<AVGOperation>();
        foreach (var item in array) {
          tempQueue.Enqueue(_GetAVGOperation(item));
        }
        avgOperations.Enqueue(tempQueue);
      }
      return avgOperations;
    }

    private void _DoOperation() {
      if (m_block > 0) {
        return;
      }

      if (m_tweenList.Count > 0) {
        while (m_tweenList.Count > 0) {
          m_tweenList.First().Complete();
        }
        return;
      }

      if (m_avgOperations.Count > 0) {
        foreach (var operatio in m_avgOperations.Dequeue()) {
          operatio.DoOperation();
        }
      } else {
        _QuitAVG();
      }
    }

    private void _QuitAVG() {
      UIManager.OpenOne("globalBlackFade", () => {
        m_onQuitAVG?.Invoke();
        UIManager.CloseOne(name);
        AudioManager.instance.PlayBgm(m_beforeBgmName);
      });
    }

    private void _InitAVG(string avgName, Action onQuitAVG) {
      m_bgImage.sprite = null;
      m_illustration.SetAlpha(0);
      m_illustration.sprite = null;
      m_bgImage.color = Color.white;
      m_dialogueCanvasGroup.alpha = 0;
      m_nameText.text = StringMap.EMPTY;
      m_dialogueText.text = StringMap.EMPTY;
      m_beforeBgmName = AudioManager.instance.GetCurrentBgmName();

      m_onQuitAVG = onQuitAVG;
      JArray arrayArray = ResourceManager.Load<JArray>(avgName);
      m_avgOperations = _InitAVGOperations(arrayArray);
    }
  }
}