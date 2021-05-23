using DG.Tweening;
using UnityEngine;

namespace Yume.AVG {
  public class SetDialogue : FadeNodeBase {
    [SerializeField]
    private string _name;
    [SerializeField]
    private bool _setSpeedBased;
    [SerializeField, TextArea]
    private string _dialogue;
    private const string USER_NAME = "[player]";

    protected override void Init() {
      base.Init();

      block = false;
    }

    protected override Tween DoTween(float fadeTime) {
      string name = _name.Replace(USER_NAME, avg.playerName);
      string dialogue = _dialogue.Replace(USER_NAME, avg.playerName);
      avg.historyDialogues += $"\n\n[{name}]:{dialogue}";
      avg.nameText.text = name;
      avg.dialogueText.text = StringMap.EMPTY;
      Tween tween = avg.dialogueText.DOText(dialogue, fadeTime);
      if (_setSpeedBased) {
        tween.SetSpeedBased();
      }
      return tween;
    }
  }
}