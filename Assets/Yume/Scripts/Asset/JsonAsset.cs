using Newtonsoft.Json;
using UnityEngine;

namespace Yume.DB {
  //因为ScriptableObject父类里有些其他不需要的字段所以需要反向剔除
  [JsonObject(MemberSerialization.OptIn)]
  public class JsonAsset : ScriptableObject {
    public TextAsset uiText;
    public TextAsset avgsText;
    public TextAsset itemsText;
    public TextAsset tasksText;
    public TextAsset cardsText;
    public TextAsset unitsText;
    public TextAsset lvMaxApMapText;
    
    [JsonProperty]
    public string ui => uiText.text;
    [JsonProperty]
    public string avgs => avgsText.text;
    [JsonProperty]
    public string items => itemsText.text;
    [JsonProperty]
    public string tasks => tasksText.text;
    [JsonProperty]
    public string cards => cardsText.text;
    [JsonProperty]
    public string units => unitsText.text;
    [JsonProperty]
    public string lvMaxApMap => lvMaxApMapText.text;
  }
}