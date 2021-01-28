using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Yume.Battle;
using Yume.Data;

namespace Yume.Editor {
  public class CardEditorWin : EditorWindow {
    private const string JSON_PATH = "Yume/DynamicAssets/Json/cardJson.json";
    private const string SPRITE_FOLDER = "../Demo/Assets/Yume/DynamicAssets/UI/Sprite/Card";
    private const string SPRITE_ASSET_PATH = "Assets/Yume/DynamicAssets/UI/Sprite/Card/{0}.png";
    private Dictionary<string, CardTemplate> m_cardDict;
    private Vector2 m_scrollPos1;
    private Vector2 m_scrollPos2;
    #region ¿¨ÅÆÊôÐÔ
    private string m_id;
    private CardType m_cardType;
    private string[] m_cardTypeNames;
    private List<GUIContent> m_icons = new List<GUIContent>();
    private string[] m_dataTypes = { "string", "int", "float", "bool" };
    private List<((int, string, object), CardPhase phase)> m_cardPhases = new List<((int, string, object), CardPhase)>();
    private Dictionary<string, List<((int, string, object), CardPhase phase)>> m_cardDictPhases = new Dictionary<string, List<((int, string, object), CardPhase phase)>>();
    #endregion

    [MenuItem("Tools/¿¨ÅÆ±à¼­Æ÷")]
    public static void ShowWin() {
      GetWindow<CardEditorWin>("¿¨ÅÆ±à¼­Æ÷").Show();
    }

    private void OnEnable() {
      m_icons = EditorWinTools.GetImageWithName(SPRITE_FOLDER, SPRITE_ASSET_PATH);
      m_cardTypeNames = typeof(CardType).GetDescriptions().ToArray();

      string text = File.ReadAllText(Path.Combine(Application.dataPath, JSON_PATH));
      m_cardDict = JsonConvert.DeserializeObject<Dictionary<string, CardTemplate>>(text);
      foreach (var key in m_cardDict.Keys) {
        m_cardDictPhases.Add(key, new List<((int, string, object), CardPhase phase)>());
        foreach (var cardPhase in m_cardDict[key].cardPhases) {
          m_cardDictPhases[key].Add((default, cardPhase));
        }
      }
    }

    private void OnGUI() {
      bool changed = false;
      EditorGUILayout.LabelField("¿¨ÅÆÄ£°å");
      m_scrollPos1 = EditorGUILayout.BeginScrollView(m_scrollPos1);
      m_id = EditorGUILayout.TextField("¿¨ÅÆid", m_id);
      m_cardType = (CardType)EditorGUILayout.Popup("¿¨ÅÆÀàÐÍ", (int)m_cardType, m_cardTypeNames);
      EditorGUILayout.LabelField("¿¨ÅÆ½×¶Î");
      if (GUILayout.Button("Ìí¼Ó½×¶Î")) {
        m_cardPhases.Add((default, new CardPhase { blackboard = new Blackboard() }));
      }
      for (int i = 0; i < m_cardPhases.Count; i++) {
        EditorGUILayout.Separator();
        EditorGUILayout.LabelField($"½×¶Î{i}");
        var item = m_cardPhases[i];
        item.phase.name = EditorGUILayout.TextField("Ãû³Æ", item.phase.name);
        item.phase.description = EditorGUILayout.TextField("ÃèÊö", item.phase.description);
        item.phase.iconFileName = m_icons[EditorGUILayout.Popup(new GUIContent("Í¼±ê"),
          Mathf.Max(0, m_icons.FindIndex(i => i.text == item.phase.iconFileName)), m_icons.ToArray())].text;
        item.phase.energyConsumption = EditorGUILayout.IntField("ºÄÄÜ", item.phase.energyConsumption);
        EditorGUILayout.LabelField("BlackboardÊý¾ÝÄ£°å");
        item.Item1.Item1 = EditorGUILayout.Popup("Êý¾ÝÀàÐÍ", item.Item1.Item1, m_dataTypes);
        item.Item1.Item2 = EditorGUILayout.TextField("¼ü", item.Item1.Item2);
        item.Item1.Item3 = EditorWinTools.EditorGUILayoutField("Öµ", m_dataTypes[item.Item1.Item1], item.Item1.Item3);
        if (GUILayout.Button("Ìí¼ÓÊý¾Ý")) {
          if (!string.IsNullOrEmpty(item.Item1.Item2) && !item.phase.blackboard.ContainsKey(item.Item1.Item2)) {
            item.phase.blackboard.Add(item.Item1.Item2, item.Item1.Item3);
          }
        }
        List<string> keys = new List<string>(item.phase.blackboard.Keys);
        foreach (var key in keys) {
          EditorGUILayout.LabelField("¼ü", key);
          EditorGUILayout.LabelField("Öµ", item.phase.blackboard[key].ToString());
          
          if (GUILayout.Button("É¾³ýÊý¾Ý")) {
            item.phase.blackboard.Remove(key);
          }
        }

        m_cardPhases[i] = item;

        if (GUILayout.Button("É¾³ý½×¶Î")) {
          m_cardPhases.RemoveAt(i);
          i--;
        }
      }

      if (GUILayout.Button("Ìí¼Ó")) {
        if (!string.IsNullOrEmpty(m_id) && !m_cardDict.ContainsKey(m_id)) {
          List<CardPhase> cardPhases = new List<CardPhase>();
          foreach (var item in m_cardPhases) {
            cardPhases.Add(item.phase);
          }
          m_cardDict.Add(m_id, new CardTemplate {
            id = m_id,
            cardType = m_cardType,
            cardPhases = cardPhases.ToArray()
          });
          changed = true;
        }
      }
      EditorGUILayout.EndScrollView();
      EditorGUILayout.Separator();

      m_scrollPos2 = EditorGUILayout.BeginScrollView(m_scrollPos2);
      List<string> cardIds = new List<string>(m_cardDict.Keys);
      foreach (var cardId in cardIds) {
        EditorGUILayout.Separator();
        CardTemplate cardTemplate = m_cardDict[cardId];
        cardTemplate.id = EditorGUILayout.TextField("¿¨ÅÆid", cardTemplate.id);
        cardTemplate.cardType = (CardType)EditorGUILayout.Popup("¿¨ÅÆÀàÐÍ", (int)cardTemplate.cardType, m_cardTypeNames);
        EditorGUILayout.LabelField("¿¨ÅÆ½×¶Î");
        List<CardPhase> cardPhases = new List<CardPhase>();
        foreach (var cardPhase in cardTemplate.cardPhases) {
          cardPhases.Add(cardPhase);
        }
        if (GUILayout.Button("Ìí¼Ó½×¶Î")) {
          CardPhase newCardPhase = new CardPhase { blackboard = new Blackboard() };
          cardPhases.Add(newCardPhase);
          m_cardDictPhases[cardId].Add((default, newCardPhase));
          changed = true;
        }
        for (int i = 0; i < cardPhases.Count; i++) {
          EditorGUILayout.Separator();
          EditorGUILayout.LabelField($"½×¶Î{i}");
          var cardPhase = cardPhases[i];
          string lastName = cardPhase.name;
          cardPhase.name = EditorGUILayout.TextField("Ãû³Æ", cardPhase.name);
          if(cardPhase.name != lastName) {
            changed = true;
          }
          string lastDesc = cardPhase.description;
          cardPhase.description = EditorGUILayout.TextField("ÃèÊö", cardPhase.description);
          if (cardPhase.description != lastDesc) {
            changed = true;
          }
          string lastIcon = cardPhase.iconFileName;
          cardPhase.iconFileName = m_icons[EditorGUILayout.Popup(new GUIContent("Í¼±ê"),
            Mathf.Max(0, m_icons.FindIndex(i => i.text == cardPhase.iconFileName)), m_icons.ToArray())].text;
          if (cardPhase.iconFileName != lastIcon) {
            changed = true;
          }
          int lastEC = cardPhase.energyConsumption;
          cardPhase.energyConsumption = EditorGUILayout.IntField("ºÄÄÜ", cardPhase.energyConsumption);
          if (cardPhase.energyConsumption != lastEC) {
            changed = true;
          }
          EditorGUILayout.LabelField("BlackboardÊý¾ÝÄ£°å");
          var item = m_cardDictPhases[cardId][i];
          item.Item1.Item1 = EditorGUILayout.Popup("Êý¾ÝÀàÐÍ", item.Item1.Item1, m_dataTypes);
          item.Item1.Item2 = EditorGUILayout.TextField("¼ü", item.Item1.Item2);
          item.Item1.Item3 = EditorWinTools.EditorGUILayoutField("Öµ", m_dataTypes[item.Item1.Item1], item.Item1.Item3);
          if (GUILayout.Button("Ìí¼ÓÊý¾Ý")) {
            if (!string.IsNullOrEmpty(item.Item1.Item2) && !item.phase.blackboard.ContainsKey(item.Item1.Item2)) {
              item.phase.blackboard.Add(item.Item1.Item2, item.Item1.Item3);
              changed = true;
            }
          }
          m_cardDictPhases[cardId][i] = item;
          List<string> keys = new List<string>(cardPhase.blackboard.Keys);
          foreach (var key in keys) {
            EditorGUILayout.LabelField("¼ü", key);
            EditorGUILayout.LabelField("Öµ", cardPhase.blackboard[key].ToString());

            if (GUILayout.Button("É¾³ýÊý¾Ý")) {
              cardPhase.blackboard.Remove(key);
              changed = true;
            }
          }

          cardPhases[i] = cardPhase;

          if (GUILayout.Button("É¾³ý½×¶Î")) {
            cardPhases.RemoveAt(i);
            m_cardDictPhases[cardId].RemoveAt(i);
            i--;
            changed = true;
          }
        }
        cardTemplate.cardPhases = cardPhases.ToArray();

        if (GUILayout.Button("É¾³ý")) {
          m_cardDict.Remove(cardId);
          m_cardDictPhases.Remove(cardId);
          changed = true;
        } else if (changed) {
          if (cardTemplate.id == cardId) {
            m_cardDict[cardTemplate.id] = cardTemplate;
            changed = true;
          } else if (!m_cardDict.ContainsKey(cardTemplate.id)) {
            m_cardDict.Remove(cardId);
            m_cardDict.Add(cardTemplate.id, cardTemplate);
            changed = true;
          }
        }
      }
      if (changed) {
        File.WriteAllText(Path.Combine(Application.dataPath, JSON_PATH), JsonConvert.SerializeObject(m_cardDict));
        AssetDatabase.Refresh();
      }
      EditorGUILayout.EndScrollView();
    }
  }
}