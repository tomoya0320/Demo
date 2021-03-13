using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using Yume.UI;

namespace Yume.AVG {
  public class PlayVideo : LeafNodeBase {
    [SerializeField]
    private VideoClip _clip;
    
    public override void Run(UIAVG avg) {
      CoroutineHelper.instance.Run(_PlayVideo(avg));
    }

    private IEnumerator _PlayVideo(UIAVG avg) {
      RenderTexture renderTexture = RenderTexture.GetTemporary(Screen.width, Screen.height);
      avg.videoRawImage.texture = renderTexture;
      avg.videoPlayer.targetTexture = renderTexture;
      
      avg.videoPlayer.gameObject.SetActiveEx(true);
      avg.videoPlayer.clip = _clip;
      avg.videoPlayer.Play();
      yield return new WaitUntil(() => !avg.videoPlayer.isPlaying);
      avg.videoPlayer.gameObject.SetActiveEx(false);
      
      avg.videoRawImage.texture = null;
      avg.videoPlayer.targetTexture = null;
      RenderTexture.ReleaseTemporary(renderTexture);
      
      avgGraph.Run(avg);
    }
  }
}