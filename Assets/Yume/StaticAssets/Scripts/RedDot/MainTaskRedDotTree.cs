using System;
using System.Collections.Generic;

namespace Yume.RedDotSystem {
  public class MainTaskRedDotTree : RedDotTree {
    public const string MAIN_TASK = "main.task";
    protected override RedDotTreeType redDotTreeType => RedDotTreeType.MAIN_TASK;

    public override void InitData() {
      redDotDict = new Dictionary<string, RedDotNode> { { MAIN_TASK, new RedDotNode(redDotTreeType, MAIN_TASK, null) } };
      foreach (TaskType type in Enum.GetValues(typeof(TaskType))) {
        redDotDict.Add(type.GetDescription(), new RedDotNode(redDotTreeType,
                         type.GetDescription(), redDotDict[MAIN_TASK]));

        foreach (var task in DataManager.playerTasks.FindAll(t => ResourceManager.Load<TaskTemplate>(t.id).type == type)) {
          if (task.state == TaskState.COMPLETED) {
            AddLeafNode(task.id, type.GetDescription());
          }
        }
      }
    }
  }
}