using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
    public TextAsset charsText;
    public TextAsset enemiesText;
    public TextAsset lvMaxApMapText;

    [JsonProperty]
    public JObject ui => JObject.Parse(uiText.text);
    [JsonProperty]
    public JObject avgs => JObject.Parse(avgsText.text);
    [JsonProperty]
    public JObject items => JObject.Parse(itemsText.text);
    [JsonProperty]
    public JObject tasks => JObject.Parse(tasksText.text);
    [JsonProperty]
    public JObject cards => JObject.Parse(cardsText.text);
    [JsonProperty]
    public JObject units {
      get {
        JObject json = new JObject();
        json.Merge(JObject.Parse(charsText.text));
        json.Merge(JObject.Parse(enemiesText.text));
        return json;
      }
    }
    [JsonProperty]
    public JObject lvMaxApMap => JObject.Parse(lvMaxApMapText.text);
  }
}