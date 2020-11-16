using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using Yume.String;

namespace Yume.Battle {
  [Serializable]
  public class CardDB {
    public string id;
    public int phase;

    public CardDB(string id, int phase) {
      this.id = id;
      this.phase = phase;
    }
  }

  [Serializable]
  public struct CardInfo {
    public enum Type {
      ATTACK, SKILL, CURSE
    }

    [Serializable]
    public struct Phase {
      public string description;
      public int energyConsumption;
      public BuffData[] buffData;
      public Blackboard blackboard;
    }

    public string id;
    public string name;
    public string iconFileName;
    public Type type;
    public Phase[] phases;
  }

  public class Card : MonoBehaviour {
    [SerializeField]
    private Image _icon;
    [SerializeField]
    private Text _energyText;    
    [SerializeField]
    private Text _phaseText;
    [SerializeField]
    private Text _descriptionText; 
    [SerializeField]
    private Text _nameText;   
    [SerializeField]
    private Text _typeText;
    
    public virtual async void OnInit(int phase, CardInfo cardInfo) {
      _nameText.text = cardInfo.name;
      _phaseText.text = phase.ToString();
      _typeText.text = StringUtil.GetCardTypeName(cardInfo.type);
      _energyText.text = cardInfo.phases[phase].energyConsumption.ToString();
      _descriptionText.text = StringUtil.GetActualDescription(cardInfo.phases[phase].description, cardInfo.phases[phase].blackboard);
      _icon.sprite = await Addressables.LoadAssetAsync<Sprite>(cardInfo.iconFileName).Task;
    }
  }
}