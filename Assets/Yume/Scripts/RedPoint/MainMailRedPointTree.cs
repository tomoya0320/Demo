using Yume.DB;

namespace Yume.RedPointSystem {
  public class MainMailRedPointTree : RedPointTree {
    public const string MAIN_MAIL = "main.mail";

    public override RedPointTree OnInit() {
      redPointTreeType = RedPointTreeType.MAIN_MAIL;

      redPointDict.Add(MAIN_MAIL, new RedPointNode(MAIN_MAIL, null));
      foreach (var mail in DBManager.instance.userMails) {
        if (!mail.isReceived) {
          InitLeafNode(mail.id, MAIN_MAIL);
        }
      }

      return this;
    }
  }
}