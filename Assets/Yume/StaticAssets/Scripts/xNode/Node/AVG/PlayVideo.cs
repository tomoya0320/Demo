using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Video;
using Yume.Resource;
using Yume.UI;

namespace Yume.AVG {
  public class PlayVideo : LeafNodeBase {
    [SerializeField]
    private VideoClip _clip;

    public override void Run(UIAVG avg) {
      CoroutineHelper.instance.Run(_PlayVideo(avg));
    }

    private IEnumerator _PlayVideo(UIAVG avg) {
      UIManager.onBackList.Add(avg.SkipVideo);
      avg.videoCanvasGroup.alpha = 1.0f;
      avg.videoCanvasGroup.interactable = true;
      avg.videoCanvasGroup.blocksRaycasts = true;

      RenderTexture renderTexture = RenderTexture.GetTemporary(Screen.width, Screen.height);
      avg.videoRawImage.material.SetFloat("_AlphaAdd", 1.0f);
      avg.videoRawImage.texture = renderTexture;
      avg.videoPlayer.targetTexture = renderTexture;

      avg.videoPlayer.clip = _clip;
      avg.videoPlayer.Play();
      yield return new WaitUntil(() => !avg.videoPlayer.isPlaying);

      UIManager.activeBack--;
      avg.videoRawImage.material.SetFloat("_AlphaAdd", 0);
      avg.videoCanvasGroup.DOFade(0, 0.5f).OnComplete(
        () => {
          avg.videoRawImage.texture = null;
          avg.videoPlayer.targetTexture = null;
          RenderTexture.ReleaseTemporary(renderTexture);
          
          avg.videoCanvasGroup.blocksRaycasts = false;
          UIManager.activeBack++;
        }
      );
      
      avg.videoCanvasGroup.interactable = false;
      UIManager.onBackList.Remove(avg.SkipVideo);
      avgGraph.Run(avg);
    }
  }
}