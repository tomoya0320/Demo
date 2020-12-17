using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Yume.UI {
  public class BlackMask : UIBase {
    private Image m_mask;
    private float m_fadeTime;
    private Action m_onFadeInComplete;
    private Action m_onFadeOutComplete;

    public override void OnInit(UIData uiData, GameObject root) {
      base.OnInit(uiData, root);
      m_mask = GetComponent<Image>("Mask");
    }

    public override void Close(params object[] args) {
      base.Close(args);

      m_mask.enabled = false;
    }

    public override void Open(params object[] args) {
      m_mask.enabled = true;
      m_fadeTime = (float)args[0];
      m_onFadeInComplete = args[1] as Action;
      m_onFadeOutComplete = args[2] as Action;
      m_mask.SetAlpha(0);
      m_mask.DOFade(1.0f, m_fadeTime).OnComplete(() => {
        m_onFadeInComplete?.Invoke();
        m_mask.DOFade(0, m_fadeTime).OnComplete(() => {
          m_onFadeOutComplete?.Invoke();
          UIManager.instance.CloseOne("blackMask");
        });
      });

      base.Open(args);
    }
  }
}