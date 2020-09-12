using Newtonsoft.Json.Linq;

namespace Yume.Server {
  public class ServerDataWrapper {
    public static JObject Wrapper<T>(T datas) where T : class {
      return JObject.FromObject(datas);
    }
  }
}