using System.Collections;

namespace Yume.Tool {
  public class CoroutineHelper : SingletonMono<CoroutineHelper> {
    public void RunCoroutine(IEnumerator enumerator) {
      StartCoroutine(enumerator);
    }
  }
}