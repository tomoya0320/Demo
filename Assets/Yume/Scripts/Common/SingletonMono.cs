using UnityEngine;

namespace Yume.Singleton {
  public abstract class SingletonMono<T> : MonoBehaviour where T : SingletonMono<T> {
    private static T s_instance;
    protected bool dontDestroyOnLoad = true;

    public static T instance {
      get {
        if (s_instance == null) {
          GameObject obj = new GameObject(typeof(T).Name);
          s_instance = obj.AddComponent<T>();
        }
        return s_instance;
      }
    }
    public static bool hasInstance {
      get {
        return s_instance != null;
      }
    }

    private void Awake() {
      if (s_instance == null) {
        s_instance = this as T;
        OnInit();
      } else {
        Debug.LogWarning("Duplicate Instacne:" + typeof(T).Name);
        Destroy(gameObject);
      }
    }

    protected virtual void OnInit() {
      if (dontDestroyOnLoad) {
        DontDestroyOnLoad(gameObject);
      }
    }
  }
}