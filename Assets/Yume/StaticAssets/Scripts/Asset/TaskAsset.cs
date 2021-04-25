using System;
using System.Collections.Generic;
using UnityEngine;

namespace Yume.Asset {
  public class TaskAsset : YScriptableObject {
    [SerializeField]
    private TaskTemplate[] _taskTemplates;

    public override Dictionary<(string, Type), object> GetActualData() {
      Dictionary<(string, Type), object> data = new Dictionary<(string, Type), object>();
      foreach (var taskTemplate in _taskTemplates) {
        data.Add((taskTemplate.id, typeof(TaskTemplate)), taskTemplate);
      }
      return data;
    }
  }
}