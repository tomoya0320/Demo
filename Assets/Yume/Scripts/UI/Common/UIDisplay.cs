using UnityEngine;

namespace Yume.UI {
  public abstract class UIDisplay : MonoBehaviour {
    [SerializeField]
    protected bool useUIStack = true;
    protected abstract bool isTweenPlaying { get; }
    public abstract void Open();
    public abstract void Close();
  }
}