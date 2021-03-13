using UnityEngine;

namespace Yume {
  public class CameraHelper : SingletonMono<CameraHelper> {
    // 保存上一帧渲染的结果以供其他地方使用
    private RenderTexture m_lastFrameRT;
    public RenderTexture lastFrameRT => m_lastFrameRT;

    private void OnRenderImage(RenderTexture src, RenderTexture dest) {
      m_lastFrameRT = src;
      Graphics.Blit(src, dest);
    }
  }
}