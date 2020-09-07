
namespace Yume.Singleton {
  public abstract class SingletonNoMono<T> where T : new() {
    private static T s_instance;

    public static T instance {
      get {
        if (s_instance == null) {
          s_instance = new T();
        }
        return s_instance;
      }
    }
    public static bool hasInstance {
      get {
        return s_instance != null;
      }
    }

    protected SingletonNoMono() {
      OnInit();
    }

    protected virtual void OnInit() { }
  }
}