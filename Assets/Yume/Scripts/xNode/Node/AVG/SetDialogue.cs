using DG.Tweening;
using UnityEngine;
using Yume.Data;
using Yume.UI;

namespace Yume.AVG {
  public class SetDialogue : FadeNodeBase {
    [SerializeField]
    private string _name;
    [SerializeField, TextArea]
    private string _dialogue;
    private const string USER_NAME = "[user]";

    protected override void Init() {
      base.Init();

      block = false;
    }

    protected override Tween DoTween(UIAVG avg, float fadeTime) {
      string name = _name.Replace(USER_NAME, DataManager.playerName);
      string dialogue = _dialogue.Replace(USER_NAME, DataManager.playerName);
      avg.historyDialogues += $"\n\n[{name}]:{dialogue}";
      avg.nameText.text = name;
      avg.dialogueText.text = StringMap.EMPTY;
      return avg.dialogueText.DOText(dialogue, fadeTime);
    }
  }
}