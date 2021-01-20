using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using UnityEngine;
using Yume.Resource;

namespace Yume.DB {
  public class JsonAsset : YScriptableObject {
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

    public JObject unitJson {
      get {
        JObject json = new JObject();
        json.Merge(JObject.Parse(_charsText.text));
        json.Merge(JObject.Parse(_enemiesText.text));
        return json;
      }
    }
    public JObject uiJson => JObject.Parse(_uiText.text);
    public JObject avgJson => JObject.Parse(_avgsText.text);
    public JObject itemJson => JObject.Parse(_itemsText.text);
    public JObject taskJson => JObject.Parse(_tasksText.text);
    public JObject cardJson => JObject.Parse(_cardsText.text);
    public JObject lvMaxApMapJson => JObject.Parse(_lvMaxApMapText.text);

    public override Dictionary<string, object> GetAssets() {
      Dictionary<string, object> assets = new Dictionary<string, object>();
      assets.Add(nameof(unitJson), unitJson);
      assets.Add(nameof(uiJson), uiJson);
      assets.Add(nameof(avgJson), avgJson);
      assets.Add(nameof(itemJson), itemJson);
      assets.Add(nameof(taskJson), taskJson);
      assets.Add(nameof(cardJson), cardJson);
      assets.Add(nameof(lvMaxApMapJson), lvMaxApMapJson);

      return assets;
    }
  }
}