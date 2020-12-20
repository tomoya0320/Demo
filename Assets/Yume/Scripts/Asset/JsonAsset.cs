using Newtonsoft.Json;
using UnityEngine;

namespace Yume.DB {
  //因为ScriptableObject父类里有些其他不需要的字段所以需要反向剔除
  [JsonObject(MemberSerialization.OptIn)]
  public class JsonAsset : ScriptableObject {
    public TextAsset itemsText;
    public TextAsset tasksText;
    public TextAsset lvMaxApMapText;
    public TextAsset cardsText;
    public TextAsset avgsText;
    public TextAsset charsText;
    public TextAsset enemiesText;
    public TextAsset uiText;
    
    [JsonProperty]
    public string items => itemsText.text;
    [JsonProperty]
    public string tasks => tasksText.text;
    [JsonProperty]
    public string lvMaxApMap => lvMaxApMapText.text;
    [JsonProperty]
    public string cards => cardsText.text;
    [JsonProperty]
    public string avgs => avgsText.text;
    [JsonProperty]
    public string chars => charsText.text;
    [JsonProperty]
    public string enemies => enemiesText.text;
    [JsonProperty]
    public string ui => uiText.text;
  }
}