using System;
using System.Collections.Generic;
using Yume.Battle;
using Yume.DB;
using Yume.RedPointSystem;

namespace Yume.MailSystem {
  [Serializable]
  public class MailDB {
    public string id;
    public string title;
    public int timeStamp;
    public string content;
    public bool isReceived;
    public string signature;
    public List<ItemDB> attachment = new List<ItemDB>();

    public MailDB(string id, string title, string content, string signature, params ItemDB[] attachment) {
      this.id = id;
      this.title = title;
      this.content = content;
      this.signature = signature; 
      if (attachment != null) {
        this.attachment.AddRange(attachment);
      }
      isReceived = this.attachment.Count == 0;
      timeStamp = MathUtil.GetTimeStamp(DateTime.Now);
    }

    public void GetAttachment() {
      isReceived = true;
      //更新RedPoint
      RedPointManager.instance.SetRedPointNum(RedPointTreeType.MAIN_MAIL, id, MainMailRedPointTree.MAIN_MAIL, 0);
      DBManager.instance.AddItems(attachment.ToArray());
    }
  }
}