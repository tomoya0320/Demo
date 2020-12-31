using System;
using Yume.Battle;
using Yume.DB;
using Yume.RedPointSystem;

namespace Yume.MailSystem {
  [Serializable]
  public class MailDB {
    public string id;
    public string title;
    public string content;
    public bool isReceived;
    public string signature;
    public DateTime dateTime;
    public ItemDB[] attachment;

    public MailDB(string id, string title, string content, string signature, params ItemDB[] attachment) {
      this.id = id;
      this.title = title;
      this.content = content;
      this.signature = signature;
      this.attachment = attachment ?? (new ItemDB[0]);
      isReceived = this.attachment.Length == 0;
      dateTime = DateTime.Now;
    }

    public void GetAttachment() {
      isReceived = true;
      //更新RedPoint
      RedPointManager.instance.SetRedPointNum(RedPointTreeType.MAIN_MAIL, id, MainMailRedPointTree.MAIN_MAIL, 0);
      DBManager.instance.AddItems(attachment);
    }
  }
}