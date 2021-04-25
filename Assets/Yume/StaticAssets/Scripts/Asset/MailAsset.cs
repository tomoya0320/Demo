using System;
using System.Collections.Generic;
using UnityEngine;

namespace Yume.Asset {
  public class MailAsset : YScriptableObject {
    [SerializeField]
    private MailTemplate[] _mailTemplates;

    public override Dictionary<(string, Type), object> GetActualData() {
      Dictionary<(string, Type), object> data = new Dictionary<(string, Type), object>();
      foreach (var mail in _mailTemplates) {
        data.Add((mail.id, typeof(MailTemplate)), mail);
      }
      return data;
    }
  }
}