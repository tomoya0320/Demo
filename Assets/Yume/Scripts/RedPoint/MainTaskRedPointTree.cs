using System;
using System.Collections.Generic;
using Yume.Data;
using Yume.Resource;

namespace Yume.RedPointSystem {
  public class MainTaskRedPointTree : RedPointTree {
    public const string MAIN_TASK = "main.task";
    protected override RedPointTreeType redPointTreeType => RedPointTreeType.MAIN_TASK;

    public override void InitData() {
      redPointDict = new Dictionary<string, RedPointNode> { { MAIN_TASK, new RedPointNode(redPointTreeType, MAIN_TASK, null) } };
      foreach (TaskType type in Enum.GetValues(typeof(TaskType))) {
        redPointDict.Add(type.GetDescription(), new RedPointNode(redPointTreeType,
                         type.GetDescription(), redPointDict[MAIN_TASK]));

        foreach (var task in DataManager.userTasks.FindAll(t => ResourceManager.Load<TaskTemplate>(t.id).type == type)) {
          if (task.state == TaskState.COMPLETED) {
            AddLeafNode(task.id, type.GetDescription());
          }
        }
      }
    }
  }
}