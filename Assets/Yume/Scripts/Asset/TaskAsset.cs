using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;
using Yume.Data;

namespace Yume.Asset {
  public class TaskAsset : YScriptableObject {
    [Serializable]
    private class Task {
      public TaskData data;
      public TaskTemplate template;
    }

    [SerializeField]
    private Task[] _tasks;

    public override Dictionary<(string, Type), object> GetActualData() {
      Dictionary<(string, Type), object> data = new Dictionary<(string, Type), object>();
      foreach (var task in _tasks) {
        var taskDataJson = JsonConvert.SerializeObject(task.data);
        var taskData = JsonConvert.DeserializeObject<TaskData>(taskDataJson);
        data.Add((task.data.id, typeof(TaskData)), taskData);
        data.Add((task.template.id, typeof(TaskTemplate)), task.template);
      }
      return data;
    }
  }
}