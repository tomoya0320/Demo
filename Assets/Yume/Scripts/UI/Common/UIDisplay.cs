using UnityEngine;

namespace Yume.UI {
  public abstract class UIDisplay : MonoBehaviour {
    [SerializeField]
    protected bool addToUIStack = true;

    public abstract void Open();
    public abstract void Close();
    public abstract bool isTweenPlaying { get; }
  }
}