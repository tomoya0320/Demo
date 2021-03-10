using System.Collections.Generic;
using Yume.Data;

namespace Yume.RedDotSystem {
  public class MainMailRedDotTree : RedDotTree {
    public const string MAIN_MAIL = "main.mail";
    protected override RedDotTreeType redDotTreeType => RedDotTreeType.MAIN_MAIL;

    public override void InitData() {
      redDotDict = new Dictionary<string, RedDotNode> { { MAIN_MAIL, new RedDotNode(redDotTreeType, MAIN_MAIL, null) } };
      foreach (var mail in DataManager.playerMails) {
        if (!mail.isReceived) {
          AddLeafNode(mail.id, MAIN_MAIL);
        }
      }
    }
  }
}