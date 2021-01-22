using System.Collections.Generic;
using Yume.Data;

namespace Yume.RedPointSystem {
  public class MainMailRedPointTree : RedPointTree {
    public const string MAIN_MAIL = "main.mail";
    protected override RedPointTreeType redPointTreeType => RedPointTreeType.MAIN_MAIL;

    public override void InitData() {
      redPointDict = new Dictionary<string, RedPointNode>();
      redPointDict.Add(MAIN_MAIL, new RedPointNode(redPointTreeType, MAIN_MAIL, null));
      foreach (var mail in DataManager.userMails) {
        if (!mail.isReceived) {
          AddLeafNode(mail.id, MAIN_MAIL);
        }
      }
    }
  }
}