using System;
using UnityEngine;
using UnityEngine.UI;
using Yume.Data;
using Object = UnityEngine.Object;

namespace Yume.UI {
  public abstract class UIBase {
    private GameObject m_root;
    private UITemplate m_uiData;
    private Animation m_animation;
    public GameObject root => m_root;
    public string name => m_uiData.name;
    public UIType uiType => m_uiData.uiType;
    private ObjectContainer m_objectContainer;
    public float cacheTime => m_uiData.cacheTime;
    protected ObjectContainer objectContainer => m_objectContainer;

    public virtual void OnInit(UITemplate uiTemplate, GameObject gameObject) {
      gameObject.SetActiveEx(false);
      
      m_root = gameObject;
      m_uiData = uiTemplate;
      m_root.name = uiTemplate.name;
      m_animation = gameObject.GetComponent<Animation>();
      m_objectContainer = gameObject.GetComponent<ObjectContainer>();
    }

    protected T GetComponent<T>(string objName) where T : Component {
      return m_objectContainer?.GetComponent<T>(objName);
    }

    protected Transform GetTransform(string objName) {
      return m_objectContainer?.GetTransform(objName);
    }

    protected GameObject GetGameObject(string objName) {
      return m_objectContainer?.GetGameObject(objName);
    }

    /// <summary>
    /// 当没有动画时直接执行callback
    /// </summary>
    /// <param name="callback"></param>
    /// <param name="args"></param>
    public virtual void Close(Action callback = null, params object[] args) {
      //保证动画完整播放
      UIManager.Block(this);

      Action action = () => {
        UIManager.Unblock(this);
        root.SetActiveEx(false);
      };
      callback = action + callback;

      var closeClip = m_animation.GetClip("close");
      if (closeClip != null) {
        CoroutineHelper.instance.PlayAnimCallback(closeClip, m_animation, callback);
      } else {
        callback();
      }
    }

    /// <summary>
    /// 当没有动画时直接执行onOpenEnd
    /// </summary>
    /// <param name="callback"></param>
    /// <param name="args"></param>
    public virtual void Open(Action callback = null, params object[] args) {
      root.SetActiveEx(true);
      //保证动画完整播放
      UIManager.Block(this);

      Action action = () => {
        UIManager.Unblock(this);
      };
      callback = action + callback;

      var opneClip = m_animation.GetClip("open");
      if (opneClip != null) {
        CoroutineHelper.instance.PlayAnimCallback(opneClip, m_animation, callback);
      } else {
        callback();
      }
    }

    public virtual void OnUpdate() { }

    /// <summary>
    /// 回收内存
    /// </summary>
    public virtual void Destroy() {
      Object.Destroy(root);
    }
  }

  /// <summary>
  /// 不受UIManager管理 由父UI管理
  /// </summary>
  public abstract class UIChildBase {
    private GameObject m_root;
    private ObjectContainer m_uiObject;
    public GameObject gameObject => m_root;

    public UIChildBase(GameObject root) {
      m_root = root;
      m_uiObject = root.GetComponent<ObjectContainer>();
    }

    protected T GetComponent<T>(string objName) where T : Component {
      return m_uiObject?.GetComponent<T>(objName);
    }

    protected GameObject GetGameObject(string objName) {
      return m_uiObject?.GetGameObject(objName);
    }    
    
    protected Transform GetTransform(string objName) {
      return m_uiObject?.GetTransform(objName);
    }

    /// <summary>
    /// 父UI打开时调用
    /// </summary>
    /// <param name="args"></param>
    public virtual void OnOpen(params object[] args) { }

    /// <summary>
    /// 父UI关闭时调用
    /// </summary>
    /// <param name="args"></param>
    public virtual void OnClose(params object[] args) { }

    public virtual void OnUpdate() { }

    public virtual void Destroy() { }
  }

  public abstract class UIGaussianBlurBg : UIBase {
    private const int ITERATIONS = 3;
    private const float BLUR_SPREAD = 0.6f;
    private const int DOWN_SAMPLE = 2;
    private RawImage m_bgRawImage;
    private RenderTexture m_renderTexture;

    private static Material s_material;
    private static Material material {
      get {
        if (s_material == null) {
          s_material = Resources.Load<Material>("GaussianBlur");
        }
        return s_material;
      }
    }

    public override void OnInit(UITemplate uiTemplate, GameObject gameObject) {
      base.OnInit(uiTemplate, gameObject);
      
      m_bgRawImage = GetComponent<RawImage>("bgRawImage");
    }

    public override void Open(Action callback = null, params object[] args) {
      m_renderTexture = RenderTexture.GetTemporary(Screen.width, Screen.height);
      Camera.main.targetTexture = m_renderTexture;
      Camera.main.Render();
      Camera.main.targetTexture = null;
      
      int rtW = m_renderTexture.width / DOWN_SAMPLE;
      int rtH = m_renderTexture.height / DOWN_SAMPLE;

      RenderTexture buffer0 = RenderTexture.GetTemporary(rtW, rtH, 0);
      buffer0.filterMode = FilterMode.Bilinear;

      Graphics.Blit(m_renderTexture, buffer0);

      for (int i = 0; i < ITERATIONS; i++)
      {
        material.SetFloat("_BlurSize", 1.0f + i * BLUR_SPREAD);

        RenderTexture buffer1 = RenderTexture.GetTemporary(rtW, rtH, 0);
                
        // 垂直
        Graphics.Blit(buffer0, buffer1, material, 0);
                
        RenderTexture.ReleaseTemporary(buffer0);
        buffer0 = buffer1;
        buffer1 = RenderTexture.GetTemporary(rtW, rtH, 0);
                
        // 水平
        Graphics.Blit(buffer0, buffer1, material, 1);
                
        RenderTexture.ReleaseTemporary(buffer0);
        buffer0 = buffer1;
      }

      Graphics.Blit(buffer0, m_renderTexture);
      RenderTexture.ReleaseTemporary(buffer0);
      m_bgRawImage.texture = m_renderTexture;
      
      base.Open(callback, args);
    }

    public override void Close(Action callback = null, params object[] args) {
      callback += () => {
        m_bgRawImage.texture = null;
        RenderTexture.ReleaseTemporary(m_renderTexture);
      };
      
      base.Close(callback, args);
    }
  }
}