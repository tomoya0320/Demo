using System;
using System.Collections.Generic;
using Yume.String;
using Yume.UI;

namespace Yume.Task {
  public enum TaskState {
    INACTIVE,
    INPROCESS,
    COMPLETED,
    FINAL
  }

  public enum TaskType {
    //TODO
  }

  public enum RequirementType {
    PROP_NUM
  }

  public enum Method {
     HAD, CONSUME_OR_ADD
  }

  [Serializable]
  public class TaskRequirement {
    public Method method;
    /// <summary>
    /// 完成任务后是否消耗所需物品
    /// </summary>
    public bool isConsumable;
    public string requirementId;
    /// <summary>
    /// 是否忽略需求的ID
    /// </summary>
    public bool isIgnoreId;
    public string description;
    public RequirementType type;
    public int requiredNum;
    public int currentNum;

    public bool Check() {
      return currentNum >= requiredNum;
    }
  }

  [Serializable]
  public class TaskReward {
    //TODO
  }

  [Serializable]
  public class Task {
    public string id;
    public string title;
    public TaskType type;
    public TaskState state;
    public string description;
    public TaskReward taskReward;
    public bool includeDialogue;
    public bool autoComplete;
    public TaskType activeTaskType;
    public string activeTaskId;
    public List<TaskRequirement> taskRequirements;

    public void Update(RequirementType type, string id, int num) {
      foreach (TaskRequirement requirement in taskRequirements) {
        if (requirement.type == type && (requirement.requirementId == id || requirement.isIgnoreId)) {
          switch (requirement.method) {
            case Method.CONSUME_OR_ADD: {
                requirement.currentNum += num;
                break;
              }
            case Method.HAD: {
                //同步现有的数量
                //TODO:switch (requirement.type)
                break;
              }
          }
        }
      }
      Check();
    }

    public void Check() {
      if (state == TaskState.INPROCESS || state == TaskState.COMPLETED) {
        bool completed = true;
        foreach (TaskRequirement requirement in taskRequirements) {
          completed = completed && requirement.Check();
        }

        if (completed && state == TaskState.INPROCESS) {
          state = TaskState.COMPLETED;
          UIController.instance.messageManager.AddMessage(new Message
          (Message.Style.FADE, string.Format(StringMap.TASK_COMPLETED, title)));

          if (autoComplete) {
            //TODO:完成任务
          }

        } else if (!completed && state == TaskState.COMPLETED) {
          state = TaskState.INPROCESS;
        }
      }
    }

    public void Complete() {
      state = TaskState.FINAL;

      //TODO:获得奖励

      if (!string.IsNullOrEmpty(activeTaskId)) {
        //TODO:激活下一个任务
      }
      foreach (TaskRequirement taskRequirement in taskRequirements) {
        if (taskRequirement.isConsumable && taskRequirement.type == RequirementType.PROP_NUM) {
          //TODO:消耗道具
        }
      }
    }
  }
}