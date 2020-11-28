using UnityEngine;

namespace Yume.DB {
  [CreateAssetMenu]
  public class GlobalJson : ScriptableObject {
    public TextAsset items;
    public TextAsset tasks;
    public TextAsset lvMaxApMap;
    public TextAsset cards;
    public TextAsset avgs;
    public TextAsset chars;
    public TextAsset enemies;
  }
}