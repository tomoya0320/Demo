using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Yume.DB {
  //因为ScriptableObject父类里有些其他不需要的字段所以需要反向剔除
  [JsonObject(MemberSerialization.OptIn)]
  public class JsonAsset : ScriptableObject {
    [SerializeField]
    private TextAsset _uiText;
    [SerializeField]
    private TextAsset _avgsText;
    [SerializeField]
    private TextAsset _itemsText;
    [SerializeField]
    private TextAsset _tasksText;
    [SerializeField]
    private TextAsset _cardsText;
    [SerializeField]
    private TextAsset _charsText;
    [SerializeField]
    private TextAsset _enemiesText;
    [SerializeField]
    private TextAsset _lvMaxApMapText;

    [JsonProperty]
    public JObject uiJson => JObject.Parse(_uiText.text);
    [JsonProperty]
    public JObject avgJson => JObject.Parse(_avgsText.text);
    [JsonProperty]
    public JObject itemJson => JObject.Parse(_itemsText.text);
    [JsonProperty]
    public JObject taskJson => JObject.Parse(_tasksText.text);
    [JsonProperty]
    public JObject cardJson => JObject.Parse(_cardsText.text);
    [JsonProperty]
    public JObject unitJson {
      get {
        JObject json = new JObject();
        json.Merge(JObject.Parse(_charsText.text));
        json.Merge(JObject.Parse(_enemiesText.text));
        return json;
      }
    }
    [JsonProperty]
    public JObject lvMaxApMapJson => JObject.Parse(_lvMaxApMapText.text);
  }
}