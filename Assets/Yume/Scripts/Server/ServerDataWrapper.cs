using Newtonsoft.Json.Linq;

namespace Yume.Server {
  public class ServerDataWrapper {
    public static JObject Wrapper<T>(T data) where T : class {
      return JObject.FromObject(data);
    }
  }
}