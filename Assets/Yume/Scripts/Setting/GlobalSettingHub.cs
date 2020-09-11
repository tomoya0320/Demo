using Yume.Singleton;

namespace Yume.Setting {
  public class GlobalSettingHub : SingletonMono<GlobalSettingHub> {
    public string serverUrl;
    public string loginServlet;
    public string registerServlet;
  }
}