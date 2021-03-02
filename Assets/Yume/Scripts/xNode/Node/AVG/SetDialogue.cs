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
      avg.nameText.text = _name.Replace(USER_NAME, DataManager.userName);
      avg.dialogueText.text = StringMap.EMPTY;
      return avg.dialogueText.DOText(_dialogue.Replace(USER_NAME, DataManager.userName), fadeTime);
    }
  }
}