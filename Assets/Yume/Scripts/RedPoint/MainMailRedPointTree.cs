using Yume.Data;

namespace Yume.RedPointSystem {
  public class MainMailRedPointTree : RedPointTree {
    public const string MAIN_MAIL = "main.mail";

    public override RedPointTree OnInit() {
      redPointTreeType = RedPointTreeType.MAIN_MAIL;

      redPointDict.Add(MAIN_MAIL, new RedPointNode(redPointTreeType, MAIN_MAIL, null));
      foreach (var mail in DataManager.instance.userMails) {
        if (!mail.isReceived) {
          AddLeafNode(mail.id, MAIN_MAIL);
        }
      }

      return this;
    }
  }
}