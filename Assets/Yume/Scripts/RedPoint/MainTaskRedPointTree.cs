using System;
using Yume.DB;
using Yume.Task;

namespace Yume.RedPointSystem {
  public class MainTaskRedPointTree : RedPointTree {
    public const string MAIN_TASK = "main.task";

    public override RedPointTree OnInit() {
      redPointTreeType = RedPointTreeType.MAIN_TASK;

      redPointDict.Add(MAIN_TASK, new RedPointNode(redPointTreeType, MAIN_TASK, null));
      foreach (TaskType type in Enum.GetValues(typeof(TaskType))) {
        redPointDict.Add(type.GetDescription(), new RedPointNode(redPointTreeType,
                         type.GetDescription(), redPointDict[MAIN_TASK]));

        foreach (var task in DBManager.instance.userTasks[type]) {
          if (task.state == TaskState.COMPLETED) {
            AddLeafNode(task.id, type.GetDescription());
          }
        }
      }

      return this;
    }
  }
}