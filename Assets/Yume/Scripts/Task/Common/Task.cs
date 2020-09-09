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

  [Serializable]
  public class TaskRequirement {
    /// <summary>
    /// flag=-1表示使用多少某某物品，flag=1表示获得多少某某物品（包括杀怪数、完成任务数）,flag=0表示拥有某种东西
    /// </summary>
    public int flag;
    /// <summary>
    /// 完成任务后是否消耗所需物品
    /// </summary>
    public bool isConsume;
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

    public void Update(string type, string id, int num) {
      foreach (TaskRequirement requirement in taskRequirements) {
        if (requirement.type.ToString() == type && (requirement.requirementId == id || requirement.isIgnoreId)) {
          switch (requirement.flag) {
            case -1: {
                requirement.currentNum -= num < 0 ? num : 0;
                break;
              }
            case 0: {
                SyncTaskRequirement(requirement);
                break;
              }
            case 1: {
                requirement.currentNum += num > 0 ? num : 0;
                break;
              }
          }
        }
      }
      Check();
    }

    //同步当前已完成的数量，只适用于诸如拥有xxx类型的任务需求
    public void SyncTaskRequirement(TaskRequirement requirement) {
      //TODO:switch (requirement.type)
    }

    public void Check() {
      if (state == TaskState.INPROCESS || state == TaskState.COMPLETED) {
        bool completed = true;
        foreach (TaskRequirement requirement in taskRequirements) {
          completed = completed && requirement.Check();
        }

        if (completed && state == TaskState.INPROCESS) {
          state = TaskState.COMPLETED;
          UIController.instance.messageManager.AddMessage(new Message(
          Message.Style.FADE, string.Format(StringMap.TASK_COMPLETED, title)));

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
        if (taskRequirement.isConsume && taskRequirement.type == RequirementType.PROP_NUM) {
          //TODO:消耗道具
        }
      }
    }
  }
}