using DG.Tweening;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Yume.Resource;

namespace Yume.UI {
  public class UIAVG : UIBase {
    #region AVGOperation
    private abstract class AVGOperation {
      protected Dictionary<string, string> dataDict = new Dictionary<string, string>();

      public AVGOperation Init(JObject rawData) {
        foreach (var kv in rawData) {
          dataDict.Add(kv.Key, kv.Value.ToString());
        }
        return this;
      }

      public abstract void DoOperation(UIAVG avg);
    }

    private class SetDialogue : AVGOperation {
      public override void DoOperation(UIAVG avg) {
        avg.m_nameText.text = dataDict["name"];
        avg.m_dialogueText.text = StringMap.EMPTY;
        Tween tween = avg.m_dialogueText.DOText(dataDict["dialogue"], 20.0f).SetSpeedBased();
        tween.OnComplete(() => avg.m_tweenList.Remove(tween));
        avg.m_tweenList.Add(tween);
      }
    }

    private abstract class AVGOperationWithFade : AVGOperation {
      protected float fadeTime;

      public override void DoOperation(UIAVG avg) {
        avg.m_block++;
        fadeTime = float.Parse(dataDict["fadeTime"]);
      }

      protected void OnFadeCompleted(UIAVG avg) {
        foreach (var item in JArray.Parse(dataDict["onFadeCompleted"])) {
          AVGOperation operation = avg._GetAVGOperation(item);
          operation.DoOperation(avg);
        }
        avg.m_block--;
      }
    }

    private class SetBackground : AVGOperationWithFade {
      public override void DoOperation(UIAVG avg) {
        base.DoOperation(avg);

        Tween fadeIn = avg.m_bgImage.DOColor(Color.black, fadeTime);
        fadeIn.OnComplete(() => {
          avg.m_tweenList.Remove(fadeIn);
          avg.m_bgImage.sprite = ResourceManager.Load<Sprite>(dataDict["bg"]);

          Tween fadeOut = avg.m_bgImage.DOColor(Color.white, fadeTime);
          fadeOut.OnComplete(() => {
            avg.m_tweenList.Remove(fadeOut);
            OnFadeCompleted(avg);
          });
          avg.m_tweenList.Add(fadeOut);
        });
        avg.m_tweenList.Add(fadeIn);
      }
    }

    private class SetDialogueVisible : AVGOperationWithFade {
      public override void DoOperation(UIAVG avg) {
        base.DoOperation(avg);

        float alpha = bool.Parse(dataDict["visible"]) ? 1.0f : 0;
        Tween tween = avg.m_dialogueCanvasGroup.DOFade(alpha, fadeTime);
        tween.OnComplete(() => {
          avg.m_tweenList.Remove(tween);
          OnFadeCompleted(avg);
        });
        avg.m_tweenList.Add(tween);
      }
    }  
    
    private class SetIllustrationVisible : AVGOperationWithFade {
      public override void DoOperation(UIAVG avg) {
        base.DoOperation(avg);

        float alpha = bool.Parse(dataDict["visible"]) ? 1.0f : 0;
        Tween tween = avg.m_illustration.DOFade(alpha, fadeTime);
        tween.OnComplete(() => {
          avg.m_tweenList.Remove(tween);
          OnFadeCompleted(avg);
        });
        avg.m_tweenList.Add(tween);
      }
    }

    private class SetIllustration : AVGOperationWithFade {
      public override void DoOperation(UIAVG avg) {
        base.DoOperation(avg);

        Tween fadeIn = avg.m_illustration.DOFade(0, fadeTime);
        fadeIn.OnComplete(() => {
          avg.m_tweenList.Remove(fadeIn);
          avg.m_illustration.sprite = ResourceManager.Load<Sprite>(dataDict["illustration"]);

          Tween fadeOut = avg.m_illustration.DOFade(1.0f, fadeTime);
          fadeOut.OnComplete(() => {
            avg.m_tweenList.Remove(fadeOut);
            OnFadeCompleted(avg);
          });
          avg.m_tweenList.Add(fadeOut);
        });
        avg.m_tweenList.Add(fadeIn);
      }
    }
    #endregion

    #region Component
    private Image m_bgImage;
    private Text m_nameText;
    private Text m_dialogueText;
    private Image m_illustration;
    private CanvasGroup m_dialogueCanvasGroup;
    #endregion

    private int m_block;
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
      GetComponent<Button>("DoOperation").onClick.AddListener(_DoOperation);
    }

    public override void Open(Action callback = null, params object[] args) {
      callback += () => {
        JArray arrayArray = ResourceManager.Load<JArray>(args[0] as string);
        m_avgOperations = _InitAVGOperations(arrayArray);
        _DoOperation();
      };

      base.Open(callback, args);
    }

    private AVGOperation _GetAVGOperation(JToken data) {
      string type = data["type"].Value<string>();
      JObject rawData = data["rawData"].Value<JObject>();
      AVGOperation operation = AssemblyTool.CreateInstance(type) as AVGOperation;
      return operation.Init(rawData);
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
          operatio.DoOperation(this);
        }
      } else {
        UIManager.OpenOne("globalBlackFade", () => UIManager.CloseOne(name));
      }
    }

    public override void Close(Action callback = null, params object[] args) {
      callback += _CleanUp;

      base.Close(callback, args);
    }

    private void _CleanUp() {
      m_dialogueCanvasGroup.alpha = 1.0f;
      m_nameText.text = StringMap.EMPTY;
      m_dialogueText.text = StringMap.EMPTY;
      m_bgImage.sprite = ResourceManager.Load<Sprite>("noSprite");
      m_illustration.sprite = ResourceManager.Load<Sprite>("noSprite");
    }
  }
}