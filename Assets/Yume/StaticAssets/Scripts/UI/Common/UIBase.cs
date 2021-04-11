using System;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Yume.UI {
  public abstract class UIBase {
    protected string cachedBgm;
    protected string cachedEffect;

    protected UITemplate uiTemplate;
    public string name => uiTemplate.name;
    public float cacheTime => uiTemplate.cacheTime;
    
    public GameObject gameObject { private set; get; }
    public Animation animation { private set; get; }
    public CanvasGroup canvasGroup { private set; get; }
    public ObjectContainer objectContainer { private set; get; }

    public virtual void OnInit(UITemplate uiTemplate, GameObject gameObject) {
      gameObject.SetActiveEx(false);

      this.gameObject = gameObject;
      this.uiTemplate = uiTemplate;
      animation = gameObject.GetComponent<Animation>();
      canvasGroup = gameObject.GetComponent<CanvasGroup>();
      objectContainer = gameObject.GetComponent<ObjectContainer>();
      if (!string.IsNullOrEmpty(this.uiTemplate.effectName)) {
        EffectManager.AddUIEffect(this.uiTemplate.effectName);
      }
    }

    protected T GetComponent<T>(string objName) where T : Component {
      return objectContainer?.GetComponent<T>(objName);
    }

    protected Transform GetTransform(string objName) {
      return objectContainer?.GetTransform(objName);
    }

    protected GameObject GetGameObject(string objName) {
      return objectContainer?.GetGameObject(objName);
    }
    
    /// <summary>
    /// 在某个动态列表里根据id获取对象
    /// 配合新手引导使用故查找是从后往前进行的
    /// </summary>
    /// <param name="list"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    public virtual RectTransform GetDynamicListObj(string list, string id) => null;

    /// <summary>
    /// 当没有动画时直接执行callback
    /// </summary>
    /// <param name="callback"></param>
    public virtual void Close(Action callback = null) {
      //保证动画完整播放
      UIManager.Block(this);

      Action action = () => {
        UIManager.Unblock(this);
        gameObject.SetActiveEx(false);
        OnHide();
      };
      callback = action + callback;

      var closeClip = animation.GetClip("close");
      if (closeClip != null) {
        CoroutineHelper.instance.PlayAnimCallback(closeClip, animation, callback);
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
      // 缓存bgm和UI特效信息
      CacheBgm();
      CacheEffect();

      gameObject.SetActiveEx(true);
      OnDisplay();

      //保证动画完整播放
      UIManager.Block(this);

      Action action = () => UIManager.Unblock(this);
      callback = action + callback;

      var opneClip = animation.GetClip("open");
      if (opneClip != null) {
        CoroutineHelper.instance.PlayAnimCallback(opneClip, animation, callback);
      } else {
        callback();
      }
    }

    public void CacheBgm() {
      if (string.IsNullOrEmpty(uiTemplate.bgm)) {
        cachedBgm = AudioManager.GetCurBgmName();
      } else {
        cachedBgm = uiTemplate.bgm;
      }
    }

    public void CacheEffect() {
      if (string.IsNullOrEmpty(uiTemplate.effectName)) {
        cachedEffect = EffectManager.GetCurUIEffect();
      } else {
        cachedEffect = uiTemplate.effectName;
      }
    }

    /// <summary>
    /// 当前UI被显示时调用 case:自己被打开或上一个UI被关闭(需等待至关闭完成)
    /// </summary>
    public virtual void OnDisplay() {
      if (!string.IsNullOrEmpty(cachedBgm)) {
        AudioManager.PlayBgm(cachedBgm);
      }
      if (!string.IsNullOrEmpty(cachedEffect)) {
        EffectManager.ShowUIEffect(cachedEffect);
      }
    }

    /// <summary>
    /// 当前UI被隐藏时调用 case:自己被关闭(需等待至关闭完成)或下一个UI被打开
    /// </summary>
    public virtual void OnHide() { }

    public virtual void OnUpdate() { }

    /// <summary>
    /// 回收内存
    /// </summary>
    public virtual void Destroy() {
      if (!string.IsNullOrEmpty(uiTemplate.effectName)) {
        EffectManager.RemoveUIEffect(uiTemplate.effectName);
      }
      Object.Destroy(gameObject);
    }
  }

  /// <summary>
  /// 不受UIManager管理 由父UI管理
  /// </summary>
  public abstract class UIChildBase {
    private GameObject m_root;
    private ObjectContainer m_uiObject;
    public GameObject gameObject => m_root;

    protected UIChildBase(GameObject root) {
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
    public virtual void OnOpen() { }

    /// <summary>
    /// 父UI关闭时调用
    /// </summary>
    public virtual void OnClose() { }

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
      Graphics.Blit(CameraHelper.instance.lastFrameRT, m_renderTexture);

      int rtW = m_renderTexture.width / DOWN_SAMPLE;
      int rtH = m_renderTexture.height / DOWN_SAMPLE;

      RenderTexture buffer0 = RenderTexture.GetTemporary(rtW, rtH);
      buffer0.filterMode = FilterMode.Bilinear;

      Graphics.Blit(m_renderTexture, buffer0);

      for (int i = 0; i < ITERATIONS; i++)
      {
        material.SetFloat("_BlurSize", 1.0f + i * BLUR_SPREAD);

        RenderTexture buffer1 = RenderTexture.GetTemporary(rtW, rtH);
                
        // 垂直
        Graphics.Blit(buffer0, buffer1, material, 0);
                
        RenderTexture.ReleaseTemporary(buffer0);
        buffer0 = buffer1;
        buffer1 = RenderTexture.GetTemporary(rtW, rtH);
                
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

    public override void Close(Action callback = null) {
      callback += () => {
        m_bgRawImage.texture = null;
        RenderTexture.ReleaseTemporary(m_renderTexture);
      };
      
      base.Close(callback);
    }
  }
}