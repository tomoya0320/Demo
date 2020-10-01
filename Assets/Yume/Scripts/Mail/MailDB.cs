using System;
using Yume.DB;

namespace Yume.MailSystem {
  [Serializable]
  public class MailDB {
    public bool isReceived;
    public string title;
    public string content;
    public string signature;
    public DateTime dateTime;
    public ItemDB[] attachment;

    public MailDB(string title, string content, string signature, params ItemDB[] attachment) {
      this.title = title;
      this.content = content;
      this.signature = signature;
      this.attachment = attachment ?? (new ItemDB[0]);
      isReceived = this.attachment.Length == 0;
      dateTime = DateTime.Now;
    }

    public void GetAttachment() {
      isReceived = true;
      DBManager.AddItems(attachment);
      DBManager.UpdateUserMails();
    }

    public void Delete() {
      DBManager.DeleteMail(this);
    }
  }
}